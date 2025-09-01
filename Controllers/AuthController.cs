using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using Matrix.Models;
using Matrix.Helpers;

namespace Matrix.Controllers
{
    /// <summary>認證相關的 Web 控制器</summary>
    public class AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IUserService userService,
        IPersonRepository personRepository,
        ICustomLocalizer localizer
    ) : Controller
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IUserService _userService = userService;
        private readonly IPersonRepository _personRepository = personRepository;
        private readonly ICustomLocalizer _localizer = localizer;
        /// <summary>確認用戶郵件</summary>
        [HttpGet, Route("/confirm/{id}")]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            try
            {
                var result = new { success = false, message = "" };

                // 嘗試解析 userId
                if (!Guid.TryParse(id, out Guid userId))
                {
                    result = new { success = false, message = _localizer["InvalidConfirmLink"] };
                }
                else
                {
                    // 查找用戶
                    var user = await _userService.GetUserEntityAsync(userId);
                    if (user == null)
                    {
                        result = new { success = false, message = _localizer["UserNotExistOrExpired"] };
                    }
                    else if (user.Status == 1)
                    {
                        result = new { success = true, message = _localizer["AccountAlreadyConfirmed"] };
                    }
                    else
                    {
                        // 更新用戶狀態為已確認
                        user.Status = 1;
                        await _userService.UpdateUserEntityAsync(user);
                        
                        // 檢查是否已有 Person 記錄，沒有則創建
                        var existingPerson = await _personRepository.GetByUserIdAsync(user.UserId);
                        if (existingPerson == null)
                        {
                            var newPerson = new Person
                            {
                                UserId = user.UserId,
                                DisplayName = user.UserName,
                                Bio = null,
                                AvatarPath = null,
                                BannerPath = null,
                                IsPrivate = 0, // 預設為公開
                                WalletAddress = null,
                                ModifyTime = null
                            };
                            
                            await _personRepository.AddAsync(newPerson);
                            await _personRepository.SaveChangesAsync();
                            _logger.LogInformation("為用戶 {Email} 創建了 Person 記錄", user.Email);
                        }
                        
                        _logger.LogInformation("用戶 {Email} 郵件確認成功", user.Email);
                        result = new { success = true, message = _localizer["EmailConfirmSuccess"] };
                    }
                }

                // 將結果存儲到 TempData，然後重定向到確認頁面
                TempData["ConfirmResult"] = System.Text.Json.JsonSerializer.Serialize(result);
                return Redirect("/confirm?from=confirm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "郵件確認過程中發生錯誤");
                var errorResult = new { success = false, message = _localizer["ConfirmProcessError"] };
                TempData["ConfirmResult"] = System.Text.Json.JsonSerializer.Serialize(errorResult);
                return Redirect("/confirm?from=confirm");
            }
        }

        /// <summary>顯示確認結果頁面</summary>
        [HttpGet, Route("/confirm")]
        public IActionResult ConfirmResult()
        {
            bool isLoading;
            bool success;
            string message;

            var serverResult = TempData["ConfirmResult"]?.ToString();
            if (!string.IsNullOrEmpty(serverResult))
            {
                try
                {
                    using var document = JsonDocument.Parse(serverResult);
                    var root = document.RootElement;
                    
                    isLoading = false;
                    success = root.GetProperty("success").GetBoolean();
                    message = root.GetProperty("message").GetString() ?? "";
                }
                catch
                {
                    isLoading = false;
                    success = false;
                    message = _localizer["ProcessingResultError"];
                }
            }
            else
            {
                // 檢查是否從確認連結重定向而來
                var fromParam = Request.Query["from"].FirstOrDefault();
                isLoading = false;
                success = false;
                
                if (fromParam == "confirm")
                {
                    message = _localizer["CannotGetResult"];
                }
                else
                {
                    message = _localizer["UseConfirmLink"];
                }
            }

            ViewBag.IsLoading = isLoading;
            ViewBag.Success = success;
            ViewBag.Message = message;
            
            // 根據狀態預處理所有本地化字串
            ViewBag.StatusIcon = success ? "✓" : "✗";
            ViewBag.Title = success ? _localizer["ConfirmSuccessTitle"] : _localizer["ConfirmFailedTitle"];
            ViewBag.Subtitle = success ? _localizer["EmailVerificationComplete"] : _localizer["VerificationProblem"];
            ViewBag.FallbackMessage = _localizer["ProcessingConfirmError"];
            
            // 成功狀態的文字
            ViewBag.VerificationCompleteLabel = _localizer["VerificationCompleteLabel"];
            ViewBag.CanUseFullFeatures = _localizer["CanUseFullFeatures"];
            
            // 失敗狀態的文字
            ViewBag.VerificationFailedLabel = _localizer["VerificationFailedLabel"];
            ViewBag.CheckLinkOrContact = _localizer["CheckLinkOrContact"];
            
            // 按鈕文字
            ViewBag.GoToLogin = _localizer["GoToLogin"];
            ViewBag.ReRegister = _localizer["ReRegister"];
            ViewBag.BackToHome = _localizer["BackToHome"];
            
            return View("~/Views/Auth/Confirm.cshtml");
        }

        /// <summary>產生包含豐富使用者資訊的 JWT Token</summary>
        public string GenerateJwtToken(UserDto user)
        {
            var jwtKey = _configuration["JWT:Key"] ??
                        throw new InvalidOperationException("JWT Key 沒有設定");
            var jwtIssuer = _configuration["JWT:Issuer"] ??
                        throw new InvalidOperationException("JWT Issuer 沒有設定");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Status", user.Status.ToString()), // 添加用戶狀態
                new Claim("DisplayName", user.Person?.DisplayName ?? user.UserName),
                new Claim("AvatarPath", user.Person?.AvatarPath ?? ""),
                new Claim("LastLoginTime", user.LastLoginTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
                new Claim("Country", user.Country ?? ""),
                new Claim("Gender", user.Gender?.ToString() ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(30), // 確保設定過期時間
                Issuer = jwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        // TODO:設定登入 Cookie
        public void SetAuthCookie(HttpResponse response, string token, bool rememberMe = false)
        {
            DateTimeOffset? expires = null;
            
            if (rememberMe)
            {
                // 勾選記住我：設定為 Cookie 能儲存的最久時間（約400天）
                expires = DateTime.UtcNow.AddDays(400);
            }
            else
            {
                // 不勾選記住我：設定為當天晚上 11:59 (本地時間)
                var today = TimeZoneHelper.GetTaipeiToday();
                var endOfDay = today.AddDays(1).AddMinutes(-1); // 當天 23:59
                expires = new DateTimeOffset(endOfDay, TimeZoneInfo.Local.GetUtcOffset(endOfDay));
            }

            // 透過設定檔控制是否在開發跨站情境下攜帶 Cookie（需 https + SameSite=None）
            var crossSiteCookies = _configuration.GetValue<bool>("Auth:CrossSiteCookies");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = crossSiteCookies ? true : false,
                SameSite = crossSiteCookies ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/",
                Expires = expires
            };

            response.Cookies.Append("AuthToken", token, cookieOptions);
            
            // 添加調試日誌
            Console.WriteLine($"\n\n=== Cookie Set ===");
            Console.WriteLine($"Token Length: {token.Length}");
            Console.WriteLine($"Remember Me: {rememberMe}");
            Console.WriteLine($"Expires: {cookieOptions.Expires}");
            Console.WriteLine($"Secure: {cookieOptions.Secure}");
            Console.WriteLine($"SameSite: {cookieOptions.SameSite}");
            Console.WriteLine($"CrossSiteCookies: {crossSiteCookies}\n\n");
        }

    }
}
