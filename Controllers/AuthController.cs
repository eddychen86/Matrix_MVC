using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace Matrix.Controllers
{
    public class AuthController(IUserService _userService, IStringLocalizer<AuthController> _localizer, ILogger<AuthController> _logger) : Controller
    {
        private static readonly string[] InvalidCredentialsError = ["Invalid user name or password."];

        private Dictionary<string, string[]> GetModelStateErrors()
        {
            return ModelState
                .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }

        [HttpGet]
        [Route("/register")]
        public ActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterViewModel model)
        {
            // 1. 記錄註冊嘗試，包含用戶名和郵箱資訊
            _logger.LogInformation("Register attempt with UserName: {UserName}, Email: {Email}", model.UserName, model.Email);

            // 2. 驗證前端傳來的資料格式是否正確
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                _logger.LogWarning("Model state is invalid: {Errors}", string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
                return Json(new { success = false, errors });
            }

            // 3. 將前端 ViewModel 轉換為後端服務層使用的 DTO 物件
            var createUserDto = new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email ?? "example@mail.com", // 暫時處理，實際應該要求輸入 Email
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm ?? model.Password
            };

            // 4. 呼叫用戶服務建立新用戶（包含密碼雜湊、重複檢查等邏輯）
            _logger.LogInformation("Debug Mode - CreateUserDto: {@CreateUserDto}", createUserDto);
            var createResult = await _userService.CreateUserAsync(createUserDto);

            // 5. 檢查用戶建立結果，如果失敗顯示具體錯誤訊息
            if (createResult.UserId == null)
            {
                var errors = new Dictionary<string, string[]> { { "UserName", createResult.Errors.ToArray() } };
                _logger.LogWarning("User creation failed: {Errors}", string.Join(", ", createResult.Errors));
                return Json(new { success = false, errors });
            }

            // 6. 註冊成功，回傳成功狀態和重導向到登入頁面的 URL
            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Login", "Auth"),
                message = "Debug mode: Skipped database write."
            });
        }

        [HttpGet]
        [Route("/login")]
        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<IActionResult> LoginApi([FromBody] LoginViewModel model)
        {
            // 1. 記錄登入嘗試，包含用戶名、密碼長度和模型驗證狀態
            _logger.LogInformation("Login attempt - UserName: {UserName}, Password Length: {PasswordLength}, ModelState Valid: {IsValid}",
              model.UserName, model.Password?.Length ?? 0, ModelState.IsValid);

            // 2. 除錯模式：記錄所有 ModelState 條目的詳細資訊
            foreach (var kvp in ModelState)
            {
                _logger.LogInformation("ModelState Key: {Key}, Valid: {Valid}, Errors: {Errors}",
                  kvp.Key, kvp.Value?.ValidationState,
                  string.Join(", ", kvp.Value?.Errors.Select(e => e.ErrorMessage) ?? []));
            }

            // 3. 驗證前端傳來的資料格式是否正確
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                _logger.LogWarning("Model state is invalid: {Errors}", string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
                return Json(new { success = false, errors });
            }

            // 4. 呼叫用戶服務驗證用戶名/郵箱和密碼是否正確
            var isValid = await _userService.ValidateUserAsync(model.UserName, model.Password ?? string.Empty);
            if (!isValid)
            {
                var errors = new Dictionary<string, string[]> { { "", InvalidCredentialsError } };
                _logger.LogWarning("User validation failed for UserName: {UserName}", model.UserName);
                return Json(new { success = false, errors });
            }

            // 5. 根據用戶名取得完整的用戶資料（先嘗試郵箱，再嘗試用戶名）
            var userDto = await _userService.GetUserByEmailAsync(model.UserName) ?? await _userService.GetUserByUsernameAsync(model.UserName);
            if (userDto == null)
            {
                return Json(new { success = false, errors = new { Error = "User not found." } });
            }

            // 6. 檢查帳號狀態（0: 未驗證, 1: 啟用, 2: 停用）
            if (userDto.Status == 0)
            {
                var errors = new Dictionary<string, string[]> { { "", [_localizer["AccountNotVerified"].ToString()] } };
                _logger.LogWarning("Account not verified for UserName: {UserName}", model.UserName);
                return Json(new { success = false, errors });
            }
            if (userDto.Status == 2)
            {
                var errors = new Dictionary<string, string[]> { { "", [_localizer["AccountDisabled"].ToString()] } };
                _logger.LogWarning("Account disabled for UserName: {UserName}", model.UserName);
                return Json(new { success = false, errors });
            }

            // 7. 產生 JWT Token，包含用戶 ID、用戶名和角色資訊
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT Key not configured.");
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new("UserId", userDto.UserId.ToString()),
                    new(ClaimTypes.Name, userDto.UserName),
                    new(ClaimTypes.Role, userDto.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                         SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature
                         )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 8. 設定 HTTP Cookie 存放 JWT Token（安全設定：HttpOnly、Secure、SameSite）
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            // 9. 如果用戶勾選「記住我」，Cookie 設定 30 天過期時間
            if (model.RememberMe)
            {
                cookieOptions.Expires = DateTime.UtcNow.AddDays(30);
            }

            // 10. 將 JWT Token 存入瀏覽器 Cookie
            Response.Cookies.Append("AuthToken", tokenString, cookieOptions);

            // 11. 登入成功，回傳成功狀態（目前為除錯模式）
            return Json(new { success = true, message = "Debug mode: Skipped validation and JWT generation." });
        }

        [HttpPost]
        [Route("/login/forgot")]
        public ActionResult ForgotPwd(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Login.cshtml", model);
            }

            /*
            SMTP 郵件發送邏輯可以在這裡實現
            */

            // 這裡可以添加忘記密碼的邏輯，例如發送重置密碼郵件等
            // 目前僅返回登入頁面
            ModelState.AddModelError("", "Forgot password functionality is not implemented yet.");
            return View("~/Views/Auth/Login.cshtml", model);
        }

        /// <summary>
        /// 檢查用戶當前的認證狀態
        /// </summary>
        /// <returns>認證狀態資訊</returns>
        [HttpGet]
        [Route("/api/auth/status")]
        public IActionResult GetAuthStatus()
        {
            // 檢查中介軟體是否已設定認證狀態
            var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;
            var isGuest = HttpContext.Items["IsGuest"] as bool? ?? false;

            if (isAuthenticated)
            {
                // 用戶已認證，回傳用戶資訊
                var userId = HttpContext.Items["UserId"] as Guid?;
                var userName = HttpContext.Items["UserName"] as string;
                var userRole = HttpContext.Items["UserRole"] as string;

                return Json(new
                {
                    success = true,
                    authenticated = true,
                    user = new
                    {
                        id = userId,
                        username = userName,
                        role = userRole
                    }
                });
            }

            // 用戶未認證（訪客狀態）
            return Json(new
            {
                success = true,
                authenticated = false,
                guest = isGuest
            });
        }

        /// <summary>
        /// 用戶登出：清除認證 Cookie
        /// </summary>
        /// <returns>登出結果</returns>
        [HttpPost]
        [Route("/api/auth/logout")]
        public IActionResult Logout()
        {
            // 清除認證 Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)  // 設定為過去時間立即過期
            };

            Response.Cookies.Append("AuthToken", "", cookieOptions);

            _logger.LogInformation("User logged out successfully");

            return Json(new
            {
                success = true,
                message = "Logged out successfully"
            });
        }
    }
}
