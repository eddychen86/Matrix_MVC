using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Matrix.Services.Interfaces;

namespace Matrix.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _conf;
        private readonly ILogger<JwtCookieMiddleware> _logger;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration conf, ILogger<JwtCookieMiddleware> logger)
        {
            _next = next;
            _conf = conf;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            // 主要邏輯稍後實作
            await _next(context);
        }

        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            // JWT 驗證羅稍後實作
            return null;
        }

        private void ClearAuthCookie(HttpContext context)
        {
            // 清除 Cookie 邏輯稍後實作
        }
    }
}