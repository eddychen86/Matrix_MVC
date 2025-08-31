# 從零開始建立 ASP.NET Core Web API（超新手友善）

不做 MVC 畫面也沒關係！這份教學帶你用最簡單方式做出一個可用的 Web API，能用瀏覽器、curl 或 Postman 測試。

---

## 1. 準備環境
- 安裝 .NET 8 SDK、VS Code（或 Visual Studio）
- 推薦 VS Code 外掛：C# Dev Kit

確認：
```
 dotnet --version
```
看到 8.x.x 代表 OK。

---

## 2. 建立 Web API 專案
```
 dotnet new webapi -n HelloApi
 cd HelloApi
 dotnet run
```
- 終端機會顯示 Swagger 位址（通常是 `https://localhost:7047/swagger`），用瀏覽器打開即可看到 API 文件，可直接點選測試。

---

## 3. 認識專案架構（最基本）
- `Program.cs`: 註冊服務、定義路由（Minimal API / Controller）
- `Controllers/`: 放 API 控制器（若使用 Controller 模式）
- `appsettings.json`: 設定檔（連線字串等）
- `Properties/launchSettings.json`: 本機執行設定

---

## 4. 新增最簡單的 API（Minimal API）
打開 `Program.cs`，在 `app.Run();` 之前加入一條路由：
```csharp
app.MapGet("/hello", () => new { message = "Hello API" });
```
執行 `dotnet run`，瀏覽器打 `http://localhost:5087/hello`（依終端機實際顯示為準），可看到 JSON：
```json
{"message":"Hello API"}
```

---

## 5.（選擇）使用 Controller 模式
建立檔案 `Controllers/HelloController.cs`：
```csharp
using Microsoft.AspNetCore.Mvc;

namespace HelloApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { message = "Hello from Controller" });
}
```
重新執行後，打 `GET /api/hello` 即可。

---

## 6. 加入 Model 與資料（DTO）
新增 `Models/TodoItem.cs`：
```csharp
namespace HelloApi.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
```

在 `Program.cs` 先做一個簡單的 In-Memory 清單：
```csharp
var todos = new List<HelloApi.Models.TodoItem>();

app.MapGet("/api/todos", () => todos);
app.MapPost("/api/todos", (HelloApi.Models.TodoItem item) => { 
    item.Id = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1; 
    todos.Add(item); 
    return Results.Created($"/api/todos/{item.Id}", item); 
});
```
用 Swagger 或 curl 測試：
```
# 新增一筆
curl -X POST http://localhost:5087/api/todos -H 'Content-Type: application/json' -d '{"title":"買牛奶","isDone":false}'
# 查詢
curl http://localhost:5087/api/todos
```

---

## 7. 接上資料庫（SQLite + EF Core）
安裝套件：
```
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite
 dotnet add package Microsoft.EntityFrameworkCore.Design
```
新增 `Data/AppDbContext.cs`：
```csharp
using HelloApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HelloApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
}
```
在 `Program.cs` 註冊 DbContext：
```csharp
using HelloApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=app.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/todos", async (AppDbContext db) => await db.Todos.AsNoTracking().ToListAsync());
app.MapPost("/api/todos", async (AppDbContext db, TodoItem item) => { db.Add(item); await db.SaveChangesAsync(); return Results.Created($"/api/todos/{item.Id}", item); });

app.Run();
```
建立資料表：
```
 dotnet ef migrations add Init
 dotnet ef database update
```

---

## 8. 加 CORS（讓前端可呼叫）
在 `Program.cs`：
```csharp
var allowAll = "AllowAll";
builder.Services.AddCors(opt => opt.AddPolicy(allowAll, p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors(allowAll);
```
這樣前端（例如 Vue）才能從不同網域呼叫 API。

---

## 9. 常見問題（超新手排查）
- Swagger 看不到：確認 `AddEndpointsApiExplorer()` 與 `UseSwagger()` 是否啟用（開發環境）
- 415 Unsupported Media Type：POST 要加 `Content-Type: application/json`
- 404 路徑不對：Minimal API 的路徑需精準比對（如 `/api/todos`）
- CORS 錯誤：開放 CORS 或限定允許來源

---

## 10. 下一步
- 拆分檔案：把路由註冊抽到擴充方法
- 加入驗證/授權（JWT）
- 部署（Zip 或容器）→ 參考 DevOps `deploy-from-zero.md` / `deploy-containers.md`

恭喜！你已經做出一個能用的 Web API！
