# ASP.NET Core 技術文件

**技術分類**: 後端開發框架  
**複雜度**: 中級到高級  
**適用情境**: Web 應用程式、Web API、微服務  

## 技術概述

ASP.NET Core 是微軟開發的跨平台、高效能、開源的 Web 開發框架。Matrix 專案使用 .NET 8.0 作為基礎框架。

## 基礎技術

### 1. 專案結構
```
Matrix/
├── Program.cs                 # 應用程式進入點
├── Matrix.csproj             # 專案設定檔
├── Controllers/              # MVC 控制器
├── Areas/                    # 區域功能（Dashboard）
├── Models/                   # 資料模型
├── Services/                 # 業務邏輯服務
├── Repository/               # 資料存取層
├── Middleware/               # 中介軟體
└── Views/                    # Razor 視圖
```

### 2. 核心設定 (Program.cs:1)

**服務註冊模式**：
- 使用 Dependency Injection 容器
- Scoped 生命週期管理：每個 HTTP 請求一個實例
- 分層註冊：Repository → Service → Controller

**關鍵配置**：
```csharp
// 資料庫連線設定
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
    }));

// JWT 驗證設定
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT 參數 */ });
```

## 進階技術

### 1. Areas 架構模式 (Areas/Dashboard/)
- **功能分離**：Dashboard 管理後台與主要應用程式分離
- **路由設定**：優先匹配 Area 路由
- **權限控制**：獨立的 Area 權限中介軟體

### 2. 中介軟體鏈 (Middleware/)
```csharp
// 執行順序很重要
app.UseMiddleware<JwtCookieMiddleware>();     // JWT Cookie 處理
app.UseDashboardAccess();                    // Dashboard 權限檢查  
app.UseAdminActivityLogging();               // 管理員活動記錄
app.UseAuthentication();
app.UseAuthorization();
```

### 3. Repository Pattern (Repository/)
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsAsync(string username, string email);
}
```

### 4. SignalR 即時通訊 (Hubs/MatrixHub.cs)
- **雙向通訊**：伺服器主動推送消息
- **群組管理**：用戶加入/離開群組
- **連線管理**：追蹤用戶上線狀態

## 使用流程

### 1. 基礎開發流程
```bash
# 1. 建立模型
Models/User.cs → 定義實體

# 2. 設定資料庫
Data/Configurations/UserConfiguration.cs → EF Core 設定

# 3. 建立資料存取
Repository/UserRepository.cs → 資料操作

# 4. 建立業務邏輯
Services/UserService.cs → 商業邏輯

# 5. 建立控制器
Controllers/UserController.cs → HTTP 請求處理
```

### 2. API 開發流程
```csharp
// 1. DTO 定義
public class UserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
}

// 2. AutoMapper 設定
cfg.CreateMap<User, UserDto>();

// 3. 控制器實作
[ApiController]
[Route("api/[controller]")]
public class UserController : ApiControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // 實作邏輯
    }
}
```

## 技術原理

### 1. Dependency Injection
- **生命週期管理**：Singleton, Scoped, Transient
- **服務解析**：建構函式注入
- **介面抽象**：降低耦合度

### 2. Model-View-Controller (MVC)
- **分離關注點**：Model（資料）、View（呈現）、Controller（邏輯）
- **路由系統**：Convention-based 和 Attribute-based 路由
- **模型綁定**：HTTP 請求自動對應到模型

### 3. Entity Framework Core
- **Code First 方法**：從程式碼產生資料庫
- **Migration 機制**：版本控制資料庫結構
- **Change Tracking**：自動追蹤實體變更

## 實際應用情境

### 1. 用戶註冊流程
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    // 1. 資料驗證
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // 2. 業務邏輯檢查
    var result = await _userRegistrationService.RegisterAsync(dto);
    
    // 3. 回應處理
    return result.Success ? Ok(result) : BadRequest(result);
}
```

### 2. JWT 身份驗證
```csharp
// JWT Cookie 中介軟體
public class JwtCookieMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // 從 Cookie 提取 JWT Token
        var token = context.Request.Cookies["AuthToken"];
        
        if (!string.IsNullOrEmpty(token))
        {
            // 設定 Authorization Header
            context.Request.Headers.Add("Authorization", $"Bearer {token}");
        }
        
        await next(context);
    }
}
```

### 3. 管理後台權限控制
```csharp
public class DashboardAccessMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value;
        
        if (path.StartsWith("/Dashboard"))
        {
            // 檢查管理員權限
            var hasPermission = await _adminPermissionService
                .HasDashboardAccessAsync(context.User);
                
            if (!hasPermission)
            {
                context.Response.StatusCode = 403;
                return;
            }
        }
        
        await next(context);
    }
}
```

## 效能優化

### 1. 資料庫優化
- **連線池**：Entity Framework Core 自動管理
- **查詢最佳化**：使用 Include() 預載關聯資料
- **非同步操作**：async/await 模式避免執行緒阻塞

### 2. 回應壓縮
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

### 3. 記憶體快取
```csharp
builder.Services.AddMemoryCache();

// 使用範例
public class UserService
{
    private readonly IMemoryCache _cache;
    
    public async Task<User> GetUserAsync(int id)
    {
        var cacheKey = $"user_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out User user))
        {
            user = await _repository.GetByIdAsync(id);
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
        }
        
        return user;
    }
}
```

## 安全性實作

### 1. 資料驗證
```csharp
[Required(ErrorMessage = "用戶名為必填")]
[StringLength(20, MinimumLength = 3, ErrorMessage = "用戶名長度必須在3-20字元之間")]
public string Username { get; set; }
```

### 2. CSRF 保護
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
```

### 3. HTTPS 重定向
```csharp
app.UseHttpsRedirection();
app.UseHsts(); // 生產環境
```

---

**建立日期**: 2025-08-29  
**適用版本**: .NET 8.0  
**相關檔案**: Program.cs, Controllers/, Services/, Repository/  
**前置需求**: .NET 8.0 SDK, SQL Server  
**學習資源**: [Microsoft ASP.NET Core 文檔](https://docs.microsoft.com/aspnet/core)