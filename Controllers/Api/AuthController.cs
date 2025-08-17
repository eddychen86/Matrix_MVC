using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;

namespace Matrix.Controllers.Api
{
    /// <summary>認證相關的 API - 狀態檢查、登出和確認信</summary>
    [Route("api/auth")]
    public class AuthController(
        ILogger<AuthController> logger,
        IEmailService emailService,
        IUserService userService,
        ICustomLocalizer localizer
    ) : ApiControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IEmailService _emailService = emailService;
        private readonly IUserService _userService = userService;
        private readonly ICustomLocalizer _localizer = localizer;
        /// <summary>檢查用戶當前的認證狀態</summary>
        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;

            if (isAuthenticated)
            {
                // 直接從 HttpContext.Items 中獲取已解析的用戶信息，避免資料庫查詢
                var userId = HttpContext.Items["UserId"] as Guid?;
                var userName = HttpContext.Items["UserName"] as string;
                var userEmail = HttpContext.Items["UserEmail"] as string;
                var userRole = HttpContext.Items["UserRole"] as int?;
                var userStatus = HttpContext.Items["UserStatus"] as int?;
                var lastLoginTime = HttpContext.Items["LastLoginTime"] as DateTime?;

                return ApiSuccess(new
                {
                    authenticated = true,
                    user = new
                    {
                        id = userId,
                        username = userName,
                        email = userEmail,
                        role = userRole ?? 0,
                        status = userStatus ?? 0,
                        isAdmin = (userRole ?? 0) >= 2,
                        isMember = (userStatus ?? 0) == 1,
                        lastLoginTime = lastLoginTime
                    }
                });
            }

            return ApiSuccess(new
            {
                authenticated = false,
                guest = HttpContext.Items["IsGuest"] as bool? ?? false
            });
        }

        /// <summary>用戶登出：清除認證 Cookie</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var expiredCookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            Response.Cookies.Append("AuthToken", "", expiredCookie);
            _logger.LogInformation("用戶登出成功");

            return ApiSuccess(message: "登出成功");
        }

        /// <summary>發送確認信</summary>
        [HttpPost("SendConfirmationEmail")]
        public async Task<IActionResult> SendConfirmationEmail(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(w => w.Value != null && w.Value.Errors.Count > 0)
                    .ToDictionary(
                        w => w.Key,
                        w => w.Value!.Errors.Select(s => s.ErrorMessage).ToArray()
                    );
                return ApiError("驗證失敗", errors);
            }

            try
            {
                // === 查找用戶以獲取 UserID ===
                var user = await _userService.GetUserByEmailAsync(model.Email ?? "");
                if (user == null)
                {
                    return ApiError(_localizer["UserNotExistPleaseRegister"]);
                }

                // === 生成確認連結 ===
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/confirm/{user.UserId}";

                // === 發送確認信 ===
                string emailBody = GenerateConfirmationEmailBody(model.UserName, confirmationLink);

                await _emailService.SendEmailAsync(
                    model.Email ?? string.Empty,
                    model.UserName,
                    _localizer["WelcomeRegisterConfirmEmail"],
                    emailBody
                );

                _logger.LogInformation("確認信已發送至: {Email}", model.Email);

                return ApiSuccess(message: _localizer["ConfirmEmailSent"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送確認信失敗");
                return ApiError(_localizer["SendConfirmEmailError"]);
            }
        }

        /// <summary>生成確認信內容</summary>
        private string GenerateConfirmationEmailBody(string userName, string confirmationLink)
        {
            // 使用 string.Format 來處理 {0} 參數
            var greeting = string.Format(_localizer["EmailGreeting"], userName);
            
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #082032 0%, #2C394B 100%); padding: 40px; text-align: center;'>
                        <h1 style='color: #FF4C29; margin: 0;'>{_localizer["EmailWelcomeTitle"]}</h1>
                        <p style='color: #FFFFFF; margin: 10px 0 0 0;'>{_localizer["EmailWelcomeSubtitle"]}</p>
                    </div>
                    
                    <div style='padding: 40px; background-color: #f9f9f9;'>
                        <h2 style='color: #082032;'>{greeting}</h2>
                        
                        <p style='color: #334756; line-height: 1.6;'>
                            {_localizer["EmailMainContent"]}
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='display: inline-block; padding: 15px 30px; background-color: #FF4C29; color: white; text-decoration: none; border-radius: 1000px; font-weight: bold;'>
                                {_localizer["EmailConfirmButton"]}
                            </a>
                        </div>
                        
                        <p style='color: #334756; font-size: 14px;'>
                            {_localizer["EmailAlternativeText"]}<br>
                            <a href='{confirmationLink}' style='color: #FF4C29;'>{confirmationLink}</a>
                        </p>
                        
                        <p style='color: #334756; font-size: 12px; margin-top: 30px; font-style: italic;'>
                            {_localizer["EmailFooterText"]}<br>
                            {_localizer["EmailBrandMotto"]}
                        </p>
                    </div>
                </div>";
        }
    }
}
