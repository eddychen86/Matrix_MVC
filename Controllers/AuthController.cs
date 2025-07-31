using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers
{
    /// <summary>認證相關的 Web 控制器</summary>
    public class AuthController(
        ILogger<AuthController> _logger,
        IConfiguration _configuration,
        IUserService _userService,
        ICustomLocalizer _localizer
    ) : WebControllerBase
    {
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

        /// <summary>產生 JWT Token (接受個別參數)</summary>
        public string GenerateJwtToken(Guid userId, string userName, string role)
        {
            var jwtKey = _configuration["JWT:Key"] ??
                        throw new InvalidOperationException("JWT Key 沒有設定");
            var jwtIssuer = _configuration["JWT:Issuer"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = jwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        // TODO:設定登入 Cookie
        public void SetAuthCookie(HttpResponse response, string token, bool rememberMe = false)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            // 記住我就設定 30 天過期
            if (rememberMe)
                cookieOptions.Expires = DateTime.UtcNow.AddDays(30);

            response.Cookies.Append("AuthToken", token, cookieOptions);
        }

    }
}