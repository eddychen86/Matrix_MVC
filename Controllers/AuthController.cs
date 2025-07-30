using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Matrix.Controllers
{
    // TODO:認證相關的 API - 狀態檢查和登出
    public class AuthController(
        ILogger<AuthController> _logger,
        IEmailService _emailService
    // IUserService _userService,
    // ApplicationDbContext _context
    ) : Controller
    {
        // TODO:檢查用戶當前的認證狀態
        [HttpGet, Route("/api/auth/status")]
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;

            if (isAuthenticated)
            {
                return Json(new
                {
                    success = true,
                    authenticated = true,
                    user = new
                    {
                        id = HttpContext.Items["UserId"] as Guid?,
                        username = HttpContext.Items["UserName"] as string,
                        role = HttpContext.Items["UserRole"] as string
                    }
                });
            }

            return Json(new
            {
                success = true,
                authenticated = false,
                guest = HttpContext.Items["IsGuest"] as bool? ?? false
            });
        }

        // TODO:用戶登出：清除認證 Cookie
        [HttpPost, Route("/api/auth/logout")]
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

            return Json(new { success = true, message = "登出成功" });
        }

        [HttpGet, Route("/Auth/GoogleCallback")]
        public IActionResult GoogleCallback()
        {
            return View();
        }

        // TODO:Google Token 交換
        public async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            // 需發送的參數
            var parameters = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "" },
                { "client_secret", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "" },
                { "redirect_uri", "https://localhost:5000/GoogleCallback" },
                { "grant_type", "authorization_code" }
            };

            // 建立 HttpClient 來發送請求
            using (var httpClient = new HttpClient())
            {
                // 將參數轉為 URL 參數字串
                var content = new FormUrlEncodedContent(parameters);

                try
                {
                    // 發送 POST 到 Google 的 Token 端點
                    var res = await httpClient.PostAsync(tokenEndpoint, content);
                    var jsonRes = await res.Content.ReadAsStringAsync();

                    // 檢查回應是否成功
                    if (res.IsSuccessStatusCode)
                    {
                        // 將JSON 反序列化為 GoogleTokenResponse DTO
                        var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonRes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return tokenData;
                    }
                    else
                    {
                        // 記錄錯誤並返回 null
                        _logger.LogError($"Token 交換失敗：{jsonRes}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // 異常處理
                    _logger.LogError($"Token 交換時發生異常：{ex}");
                    return null;
                }
            }
        }

        // TODO:取得 Google UserInfo
        public async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string accessToken)
        {
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    var res = await httpClient.GetAsync(userInfoEndpoint);
                    var jsonRes = await res.Content.ReadAsStringAsync();

                    if (res.IsSuccessStatusCode)
                    {
                        return JsonSerializer.Deserialize<GoogleUserInfo>(jsonRes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });;
                    }
                    else
                    {
                        _logger.LogError($"取得用戶資訊失敗：{jsonRes}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"取得用戶資訊時發生異常：{ex}");
                    return null;
                }
            }
        }

        // TODO:Google 授權系統
        public async Task<IActionResult> GoogleCallback(string code, string state, string error)
        {
            // 第一步：檢查是否有錯誤
            if (!string.IsNullOrEmpty(error))
            {
                // 使用者拒絕了授權，或是發生了其他錯誤，會導向到一個錯誤頁面
                ViewBag.ErrorMessage = $"授權失敗，錯誤原因：{error}";
                return Json(new { success = false, message = "AuthorizationFailed", error });
            }

            // 第二步：驗證 state (安全性檢查)
            var expectedState = HttpContext.Session.GetString("OAuthState");
            if (string.IsNullOrEmpty(expectedState) || state != expectedState)
            {
                // State 不符或遺失，可能是 CSRF 攻擊
                ViewBag.ErrorMessage = "無效的請求狀態，請重試。";
                return Json(new { success = false, message = "AuthorizationFailed", expectedState, state });
            }

            // 清除 Session 中的 state，因為它是一次性的
            HttpContext.Session.Remove("OAuthState");

            // 第三步：檢查是否收到 code
            if (string.IsNullOrEmpty(code))
            {
                // 雖然沒有 error，但也沒有 code，這是不正常的狀況
                ViewBag.ErrorMessage = "無法取得授權碼。";
                return Json(new { success = false, message = "AuthorizationFailed", code });
            }

            // 所有檢查通過！現在可以用這個 `code` 去跟 Google 交換 Token 了
            var tokens = await ExchangeCodeForTokensAsync(code);
            if (tokens == null)
            {
                _logger.LogError("無法從 Google 交換 Access Token");
                ViewBag.ErrorMessage = "授權失敗，請重試。";
                return Json(new { success = false, message = "TokenExchangeFailed" });
            }

            // 取得用戶資訊
            var userInfo = await GetGoogleUserInfoAsync(tokens.AccessToken);
            if (userInfo == null)
            {
                _logger.LogError("無法從 Google 取得用戶資訊");
                return Json(new { success = false, message = "UserIndoFailed" });
            }

            _logger.LogInformation($"Google 用戶登入：{userInfo.Email}, 姓名：{userInfo.Name}");

            /* 若要直接新建新用戶的話，那就開啟該功能
            
                // 檢查用戶是否存在於資料庫中
                var existingUser = await _userService.GetUserByEmailAsync(userInfo.Email);
                如果用戶不存在，建立新用戶
                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        Email = userInfo.Email,
                        Username = userInfo.Name,
                        GoogleId = userInfo.Id,
                        Avatar = userInfo.Picture,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    existingUser = await _userService.CreateUserAsync(newUser);
                    _logger.LogInformation($"建立新用戶：{userInfo.Email}");
                }
                else
                {
                    _logger.LogInformation($"用戶已存在：{userInfo.Email}");
                }

                // 設定認證狀態
                var cookieOpts = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    // SameSite = SameSiteMode,
                    Expires = DateTime.UtcNow.AddDays(30)
                };
                var authToken = GenerateJwtToken(existingUser.UserId, existingUser.UserName, existingUser.Role.ToString());
                Response.Cookies.Append("AuthToken", authToken, cookieOpts);
            */

            return RedirectToAction("login", "Auth");
        }

        // TODO:取得模型驗證錯誤
        private Dictionary<string, string[]> GetModelStateErrors()
        {
            return ModelState
                .Where(w => w.Value != null && w.Value.Errors.Count > 0)
                .ToDictionary(
                    w => w.Key,
                    w => w.Value!.Errors.Select(s => s.ErrorMessage).ToArray()
                );
        }

        // TODO:發送確認信
        [HttpPost]
        public async Task<IActionResult> SendConfirmationEmail(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelStateErrors() });
            }

            try
            {
                // === 生成確認 Token ===
                var confirmationToken = GenerateEmailConfirmationToken(model.UserName, model.Email ?? "");
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { email = model.Email, token = confirmationToken },
                    Request.Scheme
                );

                // === 發送確認信 ===
                string emailBody = GenerateConfirmationEmailBody(model.UserName, confirmationLink ?? "");

                await _emailService.SendEmailAsync(
                    model.Email ?? string.Empty,
                    model.UserName,
                    "歡迎註冊！請確認您的電子郵件地址",
                    emailBody
                );

                _logger.LogInformation("確認信已發送至: {Email}", model.Email);

                return Json(new
                {
                    success = true,
                    message = "確認信已發送，請檢查您的電子郵件。"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送確認信失敗");
                return Json(new
                {
                    success = false,
                    message = "發送確認信時發生錯誤，請稍後再試。"
                });
            }
        }

        // TODO:產生 JWT Token (接受個別參數)
        public string GenerateJwtToken(Guid userId, string userName, string role)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ??
                        throw new InvalidOperationException("JWT Key 沒有設定");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

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

        // TODO:生成電子郵件確認 Token（簡化版）
        private string GenerateEmailConfirmationToken(string userName, string email)
        {
            var payload = new
            {
                UserName = userName,
                Email = email,
                Purpose = "EmailConfirmation",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(jsonPayload);
            return Convert.ToBase64String(bytes);
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

        // TODO:生成確認信內容
        private string GenerateConfirmationEmailBody(string userName, string confirmationLink)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #082032 0%, #2C394B 100%); padding: 40px; text-align: center;'>
                        <h1 style='color: #FF4C29; margin: 0;'>歡迎加入 Matrix</h1>
                        <p style='color: #FFFFFF; margin: 10px 0 0 0;'>Welcome to the lighthouse.</p>
                    </div>
                    
                    <div style='padding: 40px; background-color: #f9f9f9;'>
                        <h2 style='color: #082032;'>嗨 {userName}，</h2>
                        
                        <p style='color: #334756; line-height: 1.6;'>
                            感謝您註冊 Matrix 平台！這是一個為 Web3 先鋒和深度技術愛好者打造的純淨討論空間。為了確保您的帳戶安全，請點擊下方按鈮來驗證您的電子郵件地址。
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='display: inline-block; padding: 15px 30px; background-color: #FF4C29; color: white; text-decoration: none; border-radius: 1000px; font-weight: bold;'>
                                確認電子郵件
                            </a>
                        </div>
                        
                        <p style='color: #334756; font-size: 14px;'>
                            如果按鈕無法點擊，請複製以下連結到瀏覽器：<br>
                            <a href='{confirmationLink}' style='color: #FF4C29;'>{confirmationLink}</a>
                        </p>
                        
                        <p style='color: #334756; font-size: 12px; margin-top: 30px; font-style: italic;'>
                            此連結將在 24 小時後失效。<br>
                            The world is a fog, filled with out-of-focus noise. We choose to become an eternal lighthouse.
                        </p>
                    </div>
                </div>";
        }

    }
}
