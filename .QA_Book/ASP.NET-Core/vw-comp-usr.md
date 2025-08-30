# 問題 22: ViewComponent 中引用使用者資訊 (DisplayName 和 AvatarPath)

**症狀**: ViewComponent 的 View 無法存取使用者的顯示名稱和頭像路徑，導致無法正確顯示使用者資訊

**原因**: ViewComponent 預設沒有傳遞任何資料給其 View，需要主動透過 Claims 獲取使用者資訊並傳遞給 View

**錯誤寫法**:
```csharp
// CreatePostPopupViewComponent.cs - 錯誤 1: 沒有傳遞任何資料
public IViewComponentResult Invoke()
{
    return View();
}

// CreatePostPopupViewComponent.cs - 錯誤 2: 直接使用 User.FindFirst (編譯失敗)
if (User?.Identity?.IsAuthenticated == true)
{
    viewModel.UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
    // 錯誤：IPrincipal 沒有 FindFirst 方法
}
```

```html
<!-- Default.cshtml -->
<img :src="" class="h-10 w-10 rounded-full object-cover object-center" />
<h3 class="text-white subTitle">{{  }}</h3>
```

**正確寫法**:
```csharp
// CreatePostPopupViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Matrix.ViewModels;
using System.Security.Claims;

namespace Matrix.ViewComponents
{
    public class CreatePostPopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var viewModel = new CreatePostPopupViewModel();
            
            // 正確做法：將 User 轉型為 ClaimsPrincipal
            if (User?.Identity?.IsAuthenticated == true && User is ClaimsPrincipal claimsPrincipal)
            {
                viewModel.UserName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? "";
                viewModel.UserImg = claimsPrincipal.FindFirst("AvatarPath")?.Value ?? "/static/img/default-avatar.png";
            }
            
            return View(viewModel);
        }
    }
}
```

```csharp
// MenuViewModel.cs 中新增 ViewModel
public class CreatePostPopupViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserImg { get; set; } = "/static/img/default-avatar.png";
}
```

```html
<!-- Default.cshtml -->
@model Matrix.ViewModels.CreatePostPopupViewModel

<img src="@Model.UserImg" class="h-10 w-10 rounded-full object-cover object-center" />
<h3 class="text-white subTitle">@Model.UserName</h3>
```

**解決方案**:
1. 在 ViewComponent 中透過 `User.FindFirst()` 從 Claims 獲取使用者資訊
2. 創建專用的 ViewModel 來傳遞資料
3. 在 View 中使用 `@model` 指令來接收 ViewModel
4. 使用 Razor 語法顯示使用者資訊

**注意事項**:
- **重要**: ViewComponent 中的 `User` 屬性是 `IPrincipal` 類型，不能直接調用 `FindFirst()` 方法
- 必須先將 `User` 轉型為 `ClaimsPrincipal` 才能存取 Claims
- 需要加入 `System.Security.Claims` 命名空間
- 設定預設值以處理未登入或 Claims 缺失的情況
- ClaimTypes.Name 對應使用者名稱，"AvatarPath" 是自定義 Claim
- 使用 `is ClaimsPrincipal claimsPrincipal` 進行安全的類型檢查和轉換

**相關檔案**: 
- `ViewComponents/CreatePostPopupViewComponent.cs:9-20`
- `ViewModels/MenuViewModel.cs:24-28`
- `Views/Shared/Components/CreatePostPopup/Default.cshtml:1,21-22`