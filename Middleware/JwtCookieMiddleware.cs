using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

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
        /// 執行流程：取得 Token → 驗證 Token → 查詢資料庫 → 設定 HttpContext → 繼續管道
        /// </summary>
        /// <param name="context">HTTP 請求上下文</param>
        /// <param name="userService">用戶服務：用於查詢用戶資訊和狀態</param>
        /// <param name="personRepository">個人資料倉儲：用於查詢 Person 資訊</param>
        public async Task InvokeAsync(HttpContext context, IUserService userService, IPersonRepository personRepository)
        {
            // 1. 從 Cookie 取得 JWT token
            var token = context.Request.Cookies["AuthToken"];

            // 記錄當前 cookie 內容到日誌
            _logger.LogInformation("=== Cookie Debug Info ===");
            _logger.LogInformation("Request Path: {Path}", context.Request.Path);
            _logger.LogInformation("AuthToken Cookie: {Token}", string.IsNullOrEmpty(token) ? "NOT FOUND" : "EXISTS");

            if (!string.IsNullOrEmpty(token))
            {
                // 解析並顯示 JWT payload 內容
                var payload = ExtractJwtPayload(token);
                if (payload != null)
                {
                    _logger.LogInformation(
                        "\n\nJWT Payload Content:\n- UserId (sub): {UserId}\n- UserId (direct): {UserIdDirect}\n- Expiry: {Expiry}\n\n",
                        payload.GetValueOrDefault("sub", "NOT FOUND"),
                        payload.GetValueOrDefault("UserId", "NOT FOUND"),
                        payload.GetValueOrDefault("exp", "NOT FOUND")
                    );
                }
                else
                {
                    _logger.LogWarning("Failed to parse JWT payload");
                }
            }
            _logger.LogInformation("========================");

            if (!string.IsNullOrEmpty(token))
            {
                // 2. 驗證 token
                var principal = ValidateJwtToken(token);
                _logger.LogInformation("\n\nJWT Token validation result: {IsValid}\n\n", principal != null);

                if (principal != null)
                {
                    // 3. 從 JWT Claims 直接讀取用戶資訊（避免資料庫查詢）
                    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                    var statusClaim = principal.FindFirst("Status");

                    _logger.LogInformation("\n\nUserID claim found: {Found}, Value: {Value}\n\n",
                        userIdClaim != null, userIdClaim?.Value ?? "NULL");

                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        // 4. 檢查用戶狀態（從 JWT Claims 讀取，無需查詢資料庫）
                        var userStatus = int.TryParse(statusClaim?.Value, out var status) ? status : 0;

                        _logger.LogInformation("\n\nUser status from JWT: {Status} (1=active)\n\n", userStatus);

                        // 如果能通過 JWT 驗證，且 token 未過期，則認為用戶是有效的
                        // 暫時不嚴格檢查 status，因為登入邏輯已經檢查過了
                        if (userStatus == 1 || userStatus == 0) // 暫時允許狀態 0 和 1 的用戶
                        {
                            // 5. 直接從 JWT Claims 設定用戶資訊（無需查詢資料庫）
                            context.User = principal;
                            // 🔽🔽 新增：把常用標準 Claims 補齊，避免後面 Authorize/自訂授權拿不到
                            var identity = principal.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                // NameIdentifier (sub) ─ 若沒有就用剛剛解析出的 userId
                                if (identity.FindFirst(ClaimTypes.NameIdentifier) == null)
                                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

                                // Role ─ 支援多種鍵名：ClaimTypes.Role / "Role" / "UserRole"
                                var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value
                                              ?? principal.FindFirst("Role")?.Value
                                              ?? principal.FindFirst("UserRole")?.Value;
                                if (identity.FindFirst(ClaimTypes.Role) == null && !string.IsNullOrWhiteSpace(roleStr))
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleStr));

                                // Name ─ 支援：ClaimTypes.Name / "UserName" / "DisplayName"
                                var nameStr = principal.FindFirst(ClaimTypes.Name)?.Value
                                              ?? principal.FindFirst("UserName")?.Value
                                              ?? principal.FindFirst("DisplayName")?.Value;
                                if (identity.FindFirst(ClaimTypes.Name) == null && !string.IsNullOrWhiteSpace(nameStr))
                                    identity.AddClaim(new Claim(ClaimTypes.Name, nameStr));

                                // Email ─ 支援：ClaimTypes.Email / "Email"
                                var emailStr = principal.FindFirst(ClaimTypes.Email)?.Value
                                               ?? principal.FindFirst("Email")?.Value;
                                if (identity.FindFirst(ClaimTypes.Email) == null && !string.IsNullOrWhiteSpace(emailStr))
                                    identity.AddClaim(new Claim(ClaimTypes.Email, emailStr));
                            }

                            // ==== 同步寫入 HttpContext.Items（給前端/既有擴充方法用） ====
                            // Role 轉成 int；若無則 0
                            var roleValue = principal.FindFirst(ClaimTypes.Role)?.Value
                                            ?? principal.FindFirst("Role")?.Value
                                            ?? principal.FindFirst("UserRole")?.Value;
                            int roleInt = 0;
                            _ = int.TryParse(roleValue, out roleInt);

                            // Name / Email / DisplayName
                            var userName = principal.FindFirst(ClaimTypes.Name)?.Value
                                            ?? principal.FindFirst("UserName")?.Value
                                            ?? "";
                            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                                            ?? principal.FindFirst("Email")?.Value
                                            ?? "";
                            var display = principal.FindFirst("DisplayName")?.Value
                                            ?? userName;

                            // AvatarPath：優先從 JWT Claims 取得，若無則查詢資料庫
                            var avatarFromClaim = principal.FindFirst("AvatarPath")?.Value ?? "";
                            if (string.IsNullOrWhiteSpace(avatarFromClaim))
                            {
                                try
                                {
                                    var person = await personRepository.GetByUserIdAsync(userId);
                                    avatarFromClaim = person?.AvatarPath ?? "";
                                }
                                catch { /* 最小影響，失敗時保持空字串 */ }
                            }

                            context.Items["UserId"] = userId;
                            context.Items["UserName"] = principal.FindFirst(ClaimTypes.Name)?.Value ?? "";
                            context.Items["UserEmail"] = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
                            context.Items["UserRole"] = int.TryParse(principal.FindFirst(ClaimTypes.Role)?.Value, out var role) ? role : 0;
                            context.Items["UserStatus"] = userStatus;
                            context.Items["IsAuthenticated"] = true;
                            context.Items["DisplayName"] = principal.FindFirst("DisplayName")?.Value ?? context.Items["UserName"];
                            context.Items["AvatarPath"] = avatarFromClaim;

                            // 解析 LastLoginTime
                            if (DateTime.TryParse(principal.FindFirst("LastLoginTime")?.Value, out var lastLogin))
                            {
                                context.Items["LastLoginTime"] = lastLogin;
                            }

                            _logger.LogInformation("User authenticated successfully from JWT claims: {UserName}, DisplayName: {DisplayName}",
                                context.Items["UserName"], context.Items["DisplayName"]);
                        }
                        else
                        {
                            // 用戶被停用：清除 cookie 並設定為訪客
                            SetGuestStatus(context);
                            _logger.LogWarning("User authentication failed - user disabled in JWT: {UserId}, Status: {Status}", userId, userStatus);
                        }
                    }
                    else
                    {
                        // Token 中沒有有效的用戶 ID
                        SetGuestStatus(context);
                        _logger.LogWarning("Invalid UserId in JWT token");
                    }
                }
                else
                {
                    // Token 驗證失敗
                    // ClearAuthCookie(context); // 暫時註解以測試
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
                var jwtKey = _conf["JWT:Key"];
                var jwtIssuer = _conf["JWT:Issuer"];

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
                    Path = "/", // 確保整個網站都能存取 Cookie
                    Expires = DateTime.UtcNow.AddDays(-1)  // 設定為過去時間立即過期
                };

                context.Response.Cookies.Append("AuthToken", "", cookieOptions);
                _logger.LogInformation("\n\nAuth cookie cleared for request\n\n");
            }
        }

        /// <summary>
        /// 提取 JWT payload 內容（僅用於日誌記錄，不驗證簽名）
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>JWT payload 的字典格式</returns>
        private Dictionary<string, object>? ExtractJwtPayload(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return null;

                // JWT 由三部分組成：header.payload.signature
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                // 解碼 payload（Base64URL）
                var payload = parts[1];
                // 將 Base64URL 轉換為 Base64
                var base64 = payload.Replace('-', '+').Replace('_', '/');
                // 補齊 padding
                var paddedBase64 = base64 + new string('=', (4 - base64.Length % 4) % 4);

                // 解碼並解析 JSON
                var decoded = Convert.FromBase64String(paddedBase64);
                var json = Encoding.UTF8.GetString(decoded);

                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract JWT payload for logging");
                return null;
            }
        }
    }
}
