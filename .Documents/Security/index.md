# 安全性技術文件索引

**分類**: Security & Authentication  
**技術領域**: 身份驗證、授權控制、資料安全、威脅防護  

## 📋 文件列表

### 文件 1: JWT 認證與授權完整指南
**檔案**: [`jwt-authentication.md`](./jwt-authentication.md)  
**描述**: JWT 無狀態認證系統深度解析，包含 Cookie 整合、中介軟體設計、授權控制等完整安全機制  
**關鍵字**: JWT, Authentication, Authorization, Cookie Security, Middleware, Token Management, CSRF Protection  
**相關檔案**: Middleware/JwtCookieMiddleware.cs, Controllers/Api/AuthController.cs, Services/AuthorizationService.cs  
**複雜度**: 中級到高級  

**內容概要**:
- JWT Token 生成與驗證
- Cookie 安全存放機制  
- 中介軟體認證鏈
- 角色基礎授權控制
- Token 生命週期管理
- 安全性強化措施
- 前端認證整合

---

## 🎯 學習路線

### 入門階段 (1-2 週)
1. **JWT 基礎**: 理解 JSON Web Token 結構和原理
2. **認證概念**: 學習 Authentication vs Authorization 差異
3. **Cookie 安全**: 掌握 HttpOnly、Secure、SameSite 設定

### 進階階段 (2-3 週)  
1. **中介軟體設計**: 學習認證中介軟體的實作
2. **授權策略**: 實作角色和資源基礎的授權控制
3. **Token 管理**: 掌握 Token 刷新和撤銷機制

### 專家階段 (2-3 週)
1. **安全強化**: 實作 Token 黑名單、登入限制等高級功能
2. **威脅防護**: 學習 CSRF、XSS、SQL Injection 防護
3. **安全審核**: 建立安全日誌和監控機制

---

## 🔗 技術關聯

### 核心安全技術
- **ASP.NET Core Identity**: 身份管理框架 (專案未使用，採用自定義實作)
- **Data Protection**: 資料保護 API
- **HTTPS/TLS**: 傳輸層安全
- **OWASP 安全標準**: Web 應用程式安全最佳實務

### 整合技術
- **Azure Key Vault**: 雲端金鑰管理
- **Redis**: Token 黑名單快取
- **Application Insights**: 安全事件監控
- **Azure AD**: 企業身份驗證整合

---

## 🛡️ 安全架構

### 認證流程
```
用戶登入請求
    ↓
憑證驗證 (UserService)
    ↓
JWT Token 生成 (JwtTokenService)
    ↓
HttpOnly Cookie 設定
    ↓  
JwtCookieMiddleware 處理
    ↓
Bearer Token 註入
    ↓
ASP.NET Core 認證
```

### 授權層級
```
Public Routes (無需認證)
    ↓
Authenticated Routes ([Authorize])
    ↓
Role-based Routes ([RoleAuthorization])  
    ↓
Resource-based Authorization
    ↓
Custom Authorization Policies
```

---

## 🔐 安全實作重點

### JWT Token 設計
```csharp
// Token 結構
{
  "iss": "Matrix-Application",           // 發行者
  "sub": "user-guid",                    // 用戶 ID
  "name": "username",                    // 用戶名
  "email": "user@example.com",           // 電子郵件
  "role": ["User", "Admin"],             // 角色資訊
  "jti": "unique-token-id",              // Token ID
  "iat": 1640995200,                     // 發行時間
  "exp": 1641081600                      // 過期時間
}
```

### Cookie 安全設定
```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,                       // 防止 XSS 攻擊
    Secure = Request.IsHttps,              // HTTPS 時啟用
    SameSite = SameSiteMode.Strict,        // 防止 CSRF 攻擊
    Expires = DateTime.UtcNow.AddHours(24), // 過期時間
    Path = "/"                             // Cookie 路徑
};
```

### 安全強化措施
- **登入嘗試限制**: 防止暴力破解攻擊
- **Token 黑名單**: 即時撤銷 Token 能力
- **IP 白名單**: 限制存取來源
- **速率限制**: 防止 API 濫用
- **安全標頭**: CSP、HSTS、X-Frame-Options 等

---

## ⚠️ 常見安全威脅與防護

### Cross-Site Request Forgery (CSRF)
```csharp
// CSRF Token 設定
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// API 請求中驗證 CSRF Token
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
fetch('/api/data', {
    headers: { 'RequestVerificationToken': token }
});
```

### Cross-Site Scripting (XSS)
- **輸入驗證**: 嚴格驗證所有用戶輸入
- **輸出編碼**: HTML 編碼動態內容
- **CSP 標頭**: Content Security Policy 限制執行來源
- **HttpOnly Cookie**: 防止 JavaScript 存取認證 Cookie

### SQL Injection
- **參數化查詢**: 使用 Entity Framework 參數化查詢
- **輸入驗證**: 驗證和過濾用戶輸入
- **最小權限**: 資料庫連線使用最小必要權限

---

## 🔍 安全監控與審核

### 安全事件記錄
```csharp
// 登入嘗試記錄
_logger.LogWarning("Failed login attempt for username: {Username} from IP: {IpAddress}", 
    username, ipAddress);

// 權限拒絕記錄  
_logger.LogWarning("Authorization failed for user: {UserId} accessing: {Resource}", 
    userId, resource);

// 異常活動記錄
_logger.LogError("Suspicious activity detected: {Activity} by user: {UserId}", 
    activity, userId);
```

### 健康檢查
```csharp
// 安全健康檢查
builder.Services.AddHealthChecks()
    .AddCheck<SecurityHealthCheck>("security")
    .AddCheck<CertificateHealthCheck>("certificates")
    .AddCheck<AuthServiceHealthCheck>("auth-service");
```

---

## 📚 安全最佳實務

### 開發階段
- **安全設計**: 從設計階段考慮安全性
- **代碼審查**: 重點檢查認證授權相關代碼
- **安全測試**: 進行滲透測試和漏洞掃描
- **依賴檢查**: 定期更新依賴套件，修復安全漏洞

### 部署階段
- **HTTPS 強制**: 生產環境強制使用 HTTPS
- **金鑰管理**: 使用 Azure Key Vault 管理敏感資料
- **監控告警**: 設定安全事件告警機制
- **定期備份**: 重要資料定期備份和災難恢復

### 運維階段
- **日誌監控**: 持續監控安全相關日誌
- **事件響應**: 建立安全事件響應流程
- **定期評估**: 定期進行安全風險評估
- **員工培訓**: 定期進行安全意識培訓

---

## 📚 推薦學習資源

### 官方文件與標準
- [ASP.NET Core 安全](https://docs.microsoft.com/aspnet/core/security/)
- [JWT 規範 (RFC 7519)](https://tools.ietf.org/html/rfc7519)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Microsoft 安全開發生命週期](https://www.microsoft.com/securityengineering/sdl)

### 安全工具與資源
- [JWT.io](https://jwt.io/) - JWT Token 偵錯工具
- [OWASP ZAP](https://www.zaproxy.org/) - 安全掃描工具
- [Burp Suite](https://portswigger.net/burp) - Web 應用程式安全測試
- [Snyk](https://snyk.io/) - 依賴漏洞掃描

---

**最後更新**: 2025-08-29  
**文件數量**: 1  
**總學習時間**: 5-8 週 (依個人基礎而定)