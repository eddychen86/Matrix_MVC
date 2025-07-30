using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    /// <summary>認證相關的 API - 狀態檢查和登出</summary>
    [Route("api/auth")]
    public class AuthController(ILogger<AuthController> _logger) : ApiControllerBase
    {
        /// <summary>檢查用戶當前的認證狀態</summary>
        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;

            if (isAuthenticated)
            {
                return ApiSuccess(new
                {
                    authenticated = true,
                    user = new
                    {
                        id = HttpContext.Items["UserId"] as Guid?,
                        username = HttpContext.Items["UserName"] as string,
                        role = HttpContext.Items["UserRole"] as string
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
    }
}
