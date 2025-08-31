# 從零開始建立 ASP.NET Core MVC 專案（超新手友善）

本教學專為完全沒有經驗的初學者設計（連 12 歲也能跟著做）。你只需要照著步驟做，就能從零建立一個能跑起來的 MVC 網站。

---

## 1. 準備環境

- 安裝 .NET 8 SDK（Windows/macOS/Linux 皆可）
- 安裝 VS Code（或 Visual Studio）
- 推薦外掛：C# Dev Kit、VS Code Icons

確認版本：
```
 dotnet --version
```
看到像 8.x.x 表示安裝成功。

---

## 2. 建立 MVC 專案

開啟終端機（Terminal / PowerShell）並輸入：
```
 dotnet new mvc -n HelloMvc
 cd HelloMvc
 dotnet run
```
打開瀏覽器輸入 http://localhost:5000（或指示的網址），你會看到預設首頁。

這就成功了！

---

## 3. 認識 MVC 架構

- Model：資料與商業邏輯（像資料表、資料物件）
- View：畫面（.cshtml 檔）
- Controller：接收網址請求，決定要顯示哪個 View 或回傳資料

資料夾重點：
- Controllers/ 放控制器
- Views/ 放頁面
- Models/ 放資料類別

---

## 4. 新增一個最簡單的頁面

新增控制器：`Controllers/HelloController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;

namespace HelloMvc.Controllers;

public class HelloController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Message = "哈囉，世界！";
        return View();
    }
}
```

新增 View：建立資料夾 `Views/Hello/`，新增 `Index.cshtml`
```html
@{
    ViewData["Title"] = "Hello";
}
<h1>@ViewBag.Message</h1>
```

執行 `dotnet run`，在瀏覽器開 `http://localhost:5000/Hello`，看到「哈囉，世界！」。

---

## 5. 加入 Model（資料）

建立 `Models/Post.cs`
```csharp
namespace HelloMvc.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
```

在 `HelloController` 回傳一筆資料（修改 Index）：
```csharp
public IActionResult Index()
{
    var post = new Post { Id = 1, Title = "第一篇", Content = "我的第一篇文章" };
    return View(post);
}
```

修改 View 顯示 Model：
```html
@model HelloMvc.Models.Post
<h1>@Model.Title</h1>
<p>@Model.Content</p>
```

---

## 6. 使用資料庫（SQLite + EF Core）

安裝套件：
```
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite
 dotnet add package Microsoft.EntityFrameworkCore.Design
```

建立 DbContext：`Data/AppDbContext.cs`
```csharp
using HelloMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HelloMvc.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
}
```

在 `Program.cs` 註冊資料庫：
```csharp
using HelloMvc.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=app.db"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

app.Run();
```

建立資料表：
```
 dotnet ef migrations add Init
 dotnet ef database update
```

---

## 7. 新增 CRUD 控制器（手動精簡版）

`Controllers/PostsController.cs`
```csharp
using HelloMvc.Data;
using HelloMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloMvc.Controllers;

public class PostsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
        => View(await db.Posts.AsNoTracking().ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Post model)
    {
        if (!ModelState.IsValid) return View(model);
        db.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
```

新增 View：`Views/Posts/Index.cshtml`
```html
@model IEnumerable<HelloMvc.Models.Post>
<h1>文章列表</h1>
<a href="/Posts/Create">新增文章</a>
<ul>
@foreach (var p in Model) {
    <li>@p.Title</li>
}
</ul>
```

新增 View：`Views/Posts/Create.cshtml`
```html
@model HelloMvc.Models.Post
<h1>新增文章</h1>
<form method="post">
    <input name="Title" placeholder="標題" />
    <br />
    <textarea name="Content" placeholder="內容"></textarea>
    <br />
    <button type="submit">儲存</button>
</form>
```

執行 `dotnet run`，打開 `/Posts`。

---

## 8. 常見錯誤排查（超新手版）

- 大小寫錯誤（C# 很在意大小寫）：`DisplayName` 不等於 `displayName`
- Substring 越界：裁切前先檢查長度是否夠
- DI（依賴注入）取用服務：在靜態方法內要用 `HttpContext.RequestServices.GetRequiredService<ILogger<YourController>>()`
- Razor 找不到 View：確認資料夾 `Views/控制器名/動作名.cshtml`

延伸閱讀（本專案 QA_Book）：
- Primary Constructor 注入 ILogger 導致 CS9105/CS9113 → `.QA_Book/ASP.NET-Core/common-logger-static.md`
- `auth.displayName` 大小寫錯誤 CS1061 → `.QA_Book/ASP.NET-Core/menu-auth-displayname.md`
- Substring 越界 ArgumentOutOfRange → `.QA_Book/ASP.NET-Core/menu-substring-oob.md`

---

## 9. 下一步

- 新增驗證（資料註解）：`[Required]`、`[StringLength]`
- 新增刪除、編輯、明細（CRUD 完整化）
- 學習版面：使用 `_Layout.cshtml` 統一頁首頁尾
- 學會靜態檔案與圖片上傳
- 整合前端（Vue）做互動效果

加油！你已經完成一個真正可運作的 MVC 網站雛形了！

