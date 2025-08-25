# 問題 20: 未登入用戶訪問頁面自動重定向到登入頁

**症狀**: 訪問 `/profile/{username}` 時，未登入用戶會自動重定向到 `/login` 頁面

**原因**: ProfileController 使用了 `[MemberAuthorization]` 屬性，要求用戶必須通過身份驗證

**問題分析**:
- ProfileController 標記了 `[MemberAuthorization]` 屬性
- `MemberAuthorizationAttribute` 繼承自 `RoleAuthorizationAttribute(0)`
- 在 `RoleAuthorizationAttribute.OnActionExecuting()` 中檢查 `authInfo.IsAuthenticated`
- 當 `IsAuthenticated = false` 時，執行 `context.Result = new RedirectResult("/login")`

**重定向觸發位置**:
```csharp
// Attributes/RoleAuthorizationAttribute.cs:44
if (!authInfo.IsAuthenticated)
{
    if (isApiRequest)
    {
        context.Result = new UnauthorizedObjectResult(new { message = "未授權，請先登入" });
    }
    else
    {
        // 未登入：重導向到登入頁面
        context.Result = new RedirectResult("/login");  // 👈 這裡
    }
    return;
}
```

**解決方案 1: 移除權限要求**:
```csharp
// Controllers/ProfileController.cs
// [MemberAuthorization] // 允許未登入用戶訪問個人資料頁面
public class ProfileController : Controller
{
    [HttpGet]
    [Route("/Profile")]
    [Route("/Profile/{username}")]
    public IActionResult Index(string? username = null)
    {
        return View();
    }
}
```

**解決方案 2: 使用自定義訪客屬性**:
```csharp
// Attributes/RoleAuthorizationAttribute.cs 新增
public class AllowGuestAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);  // 不執行權限檢查
    }
}

// Controllers/ProfileController.cs
[AllowGuest]  // 允許訪客存取
public class ProfileController : Controller
```

**解決方案 3: 條件性權限檢查**:
```csharp
// 在 View 中根據登入狀態顯示不同內容
@if (Context.GetAuthInfo().IsAuthenticated)
{
    // 登入用戶的完整功能
}
else
{
    // 訪客的受限功能
}
```

**相關檔案**: 
- `Controllers/ProfileController.cs:6`
- `Attributes/RoleAuthorizationAttribute.cs:33-46`

**關鍵概念**:
- ASP.NET Core 自定義授權屬性
- ActionFilter 執行順序
- HTTP 重定向 vs API 錯誤回應
- 條件性權限檢查設計模式