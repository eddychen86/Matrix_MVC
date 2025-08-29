# JWT 認證與授權技術文件

**技術分類**: 安全性與身份驗證  
**複雜度**: 中級到高級  
**適用情境**: 無狀態身份驗證、API 安全、會話管理  

## 技術概述

Matrix 專案實作 JWT (JSON Web Token) 認證系統，結合 Cookie 存放與中介軟體處理，提供安全且高效的用戶身份驗證機制。

## 基礎技術

### 1. JWT 架構組件
```
Security/
├── JWT Configuration              # JWT 基礎設定
├── JwtCookieMiddleware           # Cookie 到 Bearer Token 轉換
├── AuthController                # 認證控制器
├── AuthorizationService          # 授權服務
└── User Secrets                  # 敏感資訊管理
```

### 2. JWT 設定 (Program.cs:175-201)
```csharp
// JWT 基礎配置
var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Key or Issuer not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,              // 不驗證 Audience
        ValidateLifetime = true,               // 驗證過期時間
        ValidateIssuerSigningKey = true,       // 驗證簽名金鑰
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
```

### 3. 敏感資訊管理
```bash
# User Secrets 設定
dotnet user-secrets set "JWT:Key" "your-super-secret-jwt-signing-key-at-least-32-characters"
dotnet user-secrets set "JWT:Issuer" "Matrix-Application"
```

## 進階實作

### 1. JWT Cookie 中介軟體 (Middleware/JwtCookieMiddleware.cs)
```csharp
public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 從 Cookie 中提取 JWT Token
        var token = context.Request.Cookies["AuthToken"];

        if (!string.IsNullOrEmpty(token))
        {
            // 驗證 Token 格式
            if (IsValidJwtFormat(token))
            {
                // 設定 Authorization Header
                context.Request.Headers.Add("Authorization", $"Bearer {token}");
                _logger.LogDebug("JWT Token extracted from cookie and added to Authorization header");
            }
            else
            {
                _logger.LogWarning("Invalid JWT format found in AuthToken cookie");
                // 清除無效的 Cookie
                context.Response.Cookies.Delete("AuthToken");
            }
        }

        await _next(context);
    }

    private static bool IsValidJwtFormat(string token)
    {
        // 基本 JWT 格式驗證：三個部分由 . 分隔
        var parts = token.Split('.');
        return parts.Length == 3 && 
               parts.All(part => !string.IsNullOrEmpty(part) && IsBase64String(part));
    }

    private static bool IsBase64String(string value)
    {
        try
        {
            Convert.FromBase64String(PadBase64String(value));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string PadBase64String(string value)
    {
        int padding = 4 - (value.Length % 4);
        if (padding != 4)
        {
            value += new string('=', padding);
        }
        return value;
    }
}
```

### 2. JWT Token 生成服務
```csharp
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user, TimeSpan? expiry = null)
    {
        var jwtKey = _configuration["JWT:Key"];
        var jwtIssuer = _configuration["JWT:Issuer"];
        
        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT configuration is missing");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryTime = expiry ?? TimeSpan.FromHours(24);
        var expiration = DateTime.UtcNow.Add(expiryTime);

        // 建立 Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new("PersonId", user.PersonId?.ToString() ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // 加入角色資訊（如果有）
        if (user.Roles?.Any() == true)
        {
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }
        }

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: null,  // 不設定 audience
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT token generated for user {Username} with expiry {Expiry}", 
            user.UserName, expiration);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtKey = _configuration["JWT:Key"];
            var jwtIssuer = _configuration["JWT:Issuer"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(5) // 允許 5 分鐘時間差
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return null;
        }
    }

    public DateTime? GetTokenExpiry(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            return jsonToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }
}
```

### 3. 認證控制器實作
```csharp
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService, 
        JwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // 驗證用戶憑證
            var user = await _userService.ValidateUserAsync(loginDto.Username, loginDto.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
                return Unauthorized(new { message = "用戶名或密碼錯誤" });
            }

            // 生成 JWT Token
            var token = _jwtTokenService.GenerateToken(user);
            
            // 設定 Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                    // 防止 XSS
                Secure = Request.IsHttps,           // HTTPS 時啟用
                SameSite = SameSiteMode.Strict,     // 防止 CSRF
                Expires = DateTime.UtcNow.AddHours(24),
                Path = "/"
            };

            Response.Cookies.Append("AuthToken", token, cookieOptions);

            // 記錄登入活動
            await _userService.RecordLoginAsync(user.UserId, Request.HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("User {Username} logged in successfully", user.UserName);

            return Ok(new
            {
                success = true,
                message = "登入成功",
                user = new
                {
                    id = user.UserId,
                    username = user.UserName,
                    email = user.Email
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
            return StatusCode(500, new { message = "登入過程發生錯誤" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // 清除 Cookie
            Response.Cookies.Delete("AuthToken");

            // 記錄登出活動
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var userGuid))
            {
                await _userService.RecordLogoutAsync(userGuid);
            }

            _logger.LogInformation("User {UserId} logged out", userId);

            return Ok(new { success = true, message = "登出成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "登出過程發生錯誤" });
        }
    }

    [HttpGet("verify")]
    [Authorize]
    public async Task<IActionResult> VerifyToken()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "無效的 Token" });
            }

            // 可選：檢查用戶是否仍然存在且啟用
            if (Guid.TryParse(userId, out var userGuid))
            {
                var user = await _userService.GetUserByIdAsync(userGuid);
                if (user == null || user.IsDeleted)
                {
                    return Unauthorized(new { message = "用戶不存在或已被停用" });
                }
            }

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = userId,
                    username = username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token verification");
            return StatusCode(500, new { message = "Token 驗證過程發生錯誤" });
        }
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { message = "無效的用戶 ID" });
            }

            var user = await _userService.GetUserByIdAsync(userGuid);
            if (user == null)
            {
                return Unauthorized(new { message = "用戶不存在" });
            }

            // 生成新的 Token
            var newToken = _jwtTokenService.GenerateToken(user);

            // 更新 Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(24),
                Path = "/"
            };

            Response.Cookies.Append("AuthToken", newToken, cookieOptions);

            _logger.LogInformation("Token refreshed for user {Username}", user.UserName);

            return Ok(new { success = true, message = "Token 已更新" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "Token 更新過程發生錯誤" });
        }
    }
}
```

## 授權實作

### 1. 自訂授權屬性
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizationAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleAuthorizationAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_roles.Length > 0 && !_roles.Any(role => user.IsInRole(role)))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

// 使用範例
[RoleAuthorization("Admin", "Moderator")]
public async Task<IActionResult> AdminOnlyAction()
{
    // 只有管理員和版主可以存取
    return Ok();
}
```

### 2. 基於資源的授權
```csharp
public class ArticleAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArticleAuthorizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool CanEditArticle(Article article)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity.IsAuthenticated)
            return false;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        // 檢查是否為文章作者
        if (Guid.TryParse(userId, out var userGuid))
        {
            if (article.PersonId == userGuid)
                return true;
        }

        // 檢查是否為管理員
        if (user.IsInRole("Admin") || user.IsInRole("Moderator"))
            return true;

        return false;
    }

    public bool CanDeleteArticle(Article article)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity.IsAuthenticated)
            return false;

        // 只有管理員和作者可以刪除文章
        return CanEditArticle(article);
    }
}
```

## 安全性強化

### 1. Token 黑名單機制
```csharp
public class TokenBlacklistService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenBlacklistService> _logger;

    public TokenBlacklistService(IMemoryCache cache, ILogger<TokenBlacklistService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void BlacklistToken(string jti, DateTime expiry)
    {
        var cacheKey = $"blacklist_{jti}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiry,
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, true, cacheOptions);
        _logger.LogInformation("Token {JTI} added to blacklist", jti);
    }

    public bool IsTokenBlacklisted(string jti)
    {
        var cacheKey = $"blacklist_{jti}";
        return _cache.TryGetValue(cacheKey, out _);
    }
}

// JWT 驗證中整合黑名單檢查
public class EnhancedJwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenBlacklistService _blacklistService;

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var jti = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti) && _blacklistService.IsTokenBlacklisted(jti))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has been revoked");
                return;
            }
        }

        await _next(context);
    }
}
```

### 2. 多重安全措施
```csharp
public class SecurityEnhancementService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SecurityEnhancementService> _logger;

    // 限制登入嘗試
    public async Task<bool> IsLoginAttemptAllowedAsync(string identifier)
    {
        var cacheKey = $"login_attempts_{identifier}";
        
        if (_cache.TryGetValue(cacheKey, out int attempts))
        {
            if (attempts >= 5) // 最多 5 次嘗試
            {
                _logger.LogWarning("Too many login attempts for {Identifier}", identifier);
                return false;
            }
        }

        return true;
    }

    public async Task RecordLoginAttemptAsync(string identifier, bool success)
    {
        var cacheKey = $"login_attempts_{identifier}";
        
        if (success)
        {
            _cache.Remove(cacheKey); // 成功登入清除記錄
        }
        else
        {
            _cache.TryGetValue(cacheKey, out int attempts);
            attempts++;
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30), // 30 分鐘後重置
                Priority = CacheItemPriority.High
            };
            
            _cache.Set(cacheKey, attempts, cacheOptions);
        }
    }

    // IP 位址白名單檢查
    public bool IsIpAllowed(string ipAddress)
    {
        // 實作 IP 白名單邏輯
        var allowedIps = new[]
        {
            "127.0.0.1",
            "::1",
            // 其他允許的 IP
        };

        return allowedIps.Contains(ipAddress) || IsInAllowedRange(ipAddress);
    }

    private bool IsInAllowedRange(string ipAddress)
    {
        // 檢查是否在允許的 IP 範圍內
        // 例如：192.168.1.0/24
        return true; // 簡化實作
    }
}
```

## 前端整合

### 1. JavaScript JWT 處理
```javascript
// JWT 工具類別
class JwtManager {
    constructor() {
        this.tokenKey = 'AuthToken';
    }

    // 檢查 Token 是否存在（Cookie 中）
    hasToken() {
        return document.cookie.includes(this.tokenKey);
    }

    // 解析 JWT Token
    parseToken() {
        const token = this.getTokenFromCookie();
        if (!token) return null;

        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload;
        } catch (error) {
            console.error('Error parsing JWT token:', error);
            return null;
        }
    }

    // 檢查 Token 是否過期
    isTokenExpired() {
        const payload = this.parseToken();
        if (!payload) return true;

        const currentTime = Math.floor(Date.now() / 1000);
        return payload.exp < currentTime;
    }

    // 自動刷新 Token
    async refreshTokenIfNeeded() {
        if (this.isTokenExpired()) {
            try {
                const response = await fetch('/api/auth/refresh', {
                    method: 'POST',
                    credentials: 'include'
                });

                if (!response.ok) {
                    this.logout();
                    return false;
                }

                return true;
            } catch (error) {
                console.error('Error refreshing token:', error);
                this.logout();
                return false;
            }
        }
        return true;
    }

    // 登出
    logout() {
        fetch('/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        }).then(() => {
            window.location.href = '/login';
        });
    }

    // 從 Cookie 取得 Token
    getTokenFromCookie() {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === this.tokenKey) {
                return value;
            }
        }
        return null;
    }
}

// 使用範例
const jwtManager = new JwtManager();

// 自動檢查 Token 狀態
setInterval(async () => {
    if (jwtManager.hasToken() && jwtManager.isTokenExpired()) {
        await jwtManager.refreshTokenIfNeeded();
    }
}, 60000); // 每分鐘檢查一次
```

### 2. API 請求攔截器
```javascript
// Axios 請求攔截器
axios.interceptors.request.use(
    async (config) => {
        if (jwtManager.hasToken()) {
            const refreshed = await jwtManager.refreshTokenIfNeeded();
            if (!refreshed) {
                throw new Error('Authentication failed');
            }
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// 回應攔截器
axios.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            jwtManager.logout();
        }
        return Promise.reject(error);
    }
);
```

---

**建立日期**: 2025-08-29  
**適用版本**: ASP.NET Core 8.0, JWT Bearer Authentication  
**相關檔案**: Middleware/JwtCookieMiddleware.cs, Controllers/Api/AuthController.cs  
**安全標準**: RFC 7519 (JWT), OWASP 安全實務  
**學習資源**: [JWT.io](https://jwt.io/), [ASP.NET Core 認證](https://docs.microsoft.com/aspnet/core/security/authentication/)