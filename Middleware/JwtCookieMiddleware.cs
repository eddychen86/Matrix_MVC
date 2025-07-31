using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Matrix.Middleware
{
    // JWT Cookie 中介軟體：自動驗證 Cookie 中的 JWT Token 並設定用戶認證狀態
    public class JwtCookieMiddleware
    {
        // 下一個中介軟體的委派：用於將請求傳遞給管道中的下一個組件
        private readonly RequestDelegate _next;
        // 應用程式設定：用於讀取 JWT 相關的設定資訊
        private readonly IConfiguration _conf;
        // 日誌記錄器：用於記錄認證過程中的資訊、警告和錯誤
        private readonly ILogger<JwtCookieMiddleware> _logger;

        /// <summary>
        /// 建構子：初始化 JWT Cookie 中介軟體所需的依賴項目
        /// </summary>
        /// <param name="next">下一個中介軟體的委派</param>
        /// <param name="conf">應用程式設定</param>
        /// <param name="logger">日誌記錄器</param>
        public JwtCookieMiddleware(RequestDelegate next, IConfiguration conf, ILogger<JwtCookieMiddleware> logger)
        {
            _next = next;
            _conf = conf;
            _logger = logger;
        }

        /// <summary>
        /// 中介軟體的主要執行方法：驗證 Cookie 中的 JWT Token 並設定用戶認證狀態
        /// 執行流程：取得 Token → 驗證 Token → 檢查用戶狀態 → 設定 HttpContext → 繼續管道
        /// </summary>
        /// <param name="context">HTTP 請求上下文</param>
        /// <param name="userService">用戶服務：用於查詢用戶資訊和狀態</param>
        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            // 1. 從 Cookie 取得 JWT token
            var token = context.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                // 2. 驗證 token
                var principal = ValidateJwtToken(token);

                if (principal != null)
                {
                    // 3. 從 token 中解析 user id
                    var userIdClaim = principal.FindFirst("UserId");
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        // 4. 檢查用戶存在性與狀態
                        var userDto = await userService.GetUserAsync(userId);

                        // Status == 1 表示啟用
                        if (userDto != null && userDto.Status == 1)
                        {
                            // 5. 認證成功：設定 HttpContext
                            context.User = principal;
                            context.Items["UserId"] = userId;
                            context.Items["UserName"] = userDto.UserName;
                            context.Items["UserRole"] = userDto.Role;
                            context.Items["IsAuthenticated"] = true;

                            _logger.LogInformation("User authenticated successfully: {UserName}", userDto.UserName);
                        }
                        else
                        {
                            // 用戶不存在或被停用：清除 cookie 並設定為訪客
                            ClearAuthCookie(context);
                            SetGuestStatus(context);
                            _logger.LogWarning("User authentication failed - user not found or disabled: {UserId}", userId);
                        }
                    }
                    else
                    {
                        // Token 中沒有有效的用戶 ID
                        ClearAuthCookie(context);
                        SetGuestStatus(context);
                        _logger.LogWarning("Invalid UserId in JWT token");
                    }
                }
                else
                {
                    // Token 驗證失敗
                    ClearAuthCookie(context);
                    SetGuestStatus(context);
                    _logger.LogWarning("JWT token validation failed");
                }
            }
            else
            {
                // 沒有 Token：設為訪客狀態
                SetGuestStatus(context);
            }

            // 繼續執行下一個中介軟體
            await _next(context);
        }

        /// <summary>   
        /// 設定訪客狀態：將 HttpContext 標記為未認證的訪 客狀態
        /// </summary>
        /// <param name="context">HTTP 請求上下文</param>
        private void SetGuestStatus(HttpContext context)
        {
            context.Items["IsAuthenticated"] = false;
            context.Items["IsGuest"] = true;
        }

        /// <summary>
        /// 驗證 JWT Token：檢查 Token 的格式、簽章、過期時間等是否有效
        /// </summary>
        /// <param name="token">要驗證的 JWT Token 字串</param>
        /// <returns>驗證成功回傳 ClaimsPrincipal，失敗回傳 null</returns>
        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            try
            {
                // 取得 JWT 設定
                var jwtKey = _conf["Jwt:Key"];
                var jwtIssuer = _conf["Jwt:Issuer"];

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
                {
                    _logger.LogError("JWT Key or Issuer not configured in appsettings.json or user secrets.");
                    return null;
                }

                // 設定 Token 驗證參數
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var vaildationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false,   // 如果不需要驗證 audience
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero   // 不允許時間偏差
                };

                // 驗證 token
                var principal = tokenHandler.ValidateToken(token, vaildationParameters, out SecurityToken validatedToken);

                // 確認是 JWT Token
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase)
                )
                {
                    return principal;
                }

                return null;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("JWT token validation failed: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 清除認證 Cookie：將瀏覽器中的 AuthToken Cookie 設為過期狀態以強制刪除
        /// </summary>
        /// <param name="context">HTTP 請求上下文</param>
        private void ClearAuthCookie(HttpContext context)
        {
            // 清除認證 Cookie
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,    // 防止 JavaScript 存取
                    Secure = true,      // 只在 HTTPS 下傳輸
                    SameSite = SameSiteMode.Strict,  // 防止 CSRF 攻擊
                    Expires = DateTime.UtcNow.AddDays(-1)  // 設定為過去時間立即過期
                };

                context.Response.Cookies.Append("AuthToken", "", cookieOptions);
                _logger.LogInformation("Auth cookie cleared");
            }
        }
    }
}