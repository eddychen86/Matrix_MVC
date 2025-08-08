# Repository Guidelines

## 專案結構與模組
- `Controllers/`, `Views/`, `ViewModels/`: MVC 端點、Razor 視圖與頁面綁定模型。
- `Models/`, `DTOs/`: 資料實體與對外/對內傳輸結構（不含商業邏輯）。
- `Services/`, `Repository/`, `Data/`: 商業邏輯、資料存取與 EF Core 組態/DbContext。
- `Migrations/`: EF Core 遷移（自動產生檔，不要手動修改）。
- `Areas/`: 區域化模組（如後台/子領域）。
- `wwwroot/`: 靜態資產；LibMan 管理的 `lib/`，以及 `scss/`、`css/`。
- 其他：`Middleware/`, `Extensions/`, `Mappings/`, `Attributes/`, `ViewComponents/`。

## 品牌與語氣（來自 CLAUDE.md）
- 核心調性：神秘、詩意、自信、精準；以「燈塔/濃霧/回聲/訊號」等隱喻。
- 介面原則：圓角 8–12px、按鈕膠囊形（`border-radius: 1000px`）。
- 色彩：深色基底（#082032）、輔色海軍藍（#2C394B）、灰（#334756）、重點橘（#FF4C29）。
- Icon：使用 Lucide（線條風格，圓角）；字型以 Inter / Noto Sans TC。

## 建置、測試與開發指令
- 還原/建置/執行：`dotnet restore`、`dotnet build`、`dotnet run` 或 `dotnet watch run`。
- 前端資產（macOS/Linux）：
  - `sass "wwwroot/scss/main.scss" "wwwroot/css/components.css" -w --no-source-map`
  - `./tailwindcss -i "wwwroot/css/tailwind.css" -o "wwwroot/css/site.css" -w`
- LibMan：`dotnet tool run libman restore`（安裝到 `wwwroot/lib`）。
- EF Core：`dotnet ef migrations add <Name>`、`dotnet ef database update`。

## 程式風格與命名
- C#/.NET 8：啟用可空參考與隱式使用；縮排 4 空白、花括號換行。
- 命名：類型/方法 PascalCase；區域變數/參數 camelCase。
- 後綴：`...Controller`、`...Service`、`...Repository`、`...Dto`、`...ViewModel`。
- 檔案：一檔一公用型別；提交前執行 `dotnet format`。

## 測試指引
- 建議新增 `Matrix.Tests/`（xUnit）。命名：`Class_Method_ShouldExpectedBehavior`。
- 以服務/儲存層與驗證為覆蓋重點；執行 `dotnet test`。

## Commit 與 PR 原則
- 採 Conventional Commits：`feat:`、`fix:`、`refactor:`、`perf:`、`docs:`…
- PR 請附：變更摘要、範圍、關聯 Issue、遷移注意事項、手動測試步驟；介面變更加圖/GIF。
- 小步提交、聚焦單一目的。

## 安全與設定
- 設定：`appsettings*.json`；機密使用 User Secrets/環境變數，不提交到版本庫。
- 資料庫一律走 Code First 流程：改 Model/Config → 新增 Migration → 更新資料庫。

## 架構筆記
- 薄控制器 → 服務層商業邏輯 → Repository/EF 存取；對外交互使用 DTO。
- 前端使用 Tailwind + DaisyUI；遵循品牌色與元件規範，Icon 統一使用 Lucide。

## View Components 與 Partial 放置規範
- View Components 檢視放在：`Views/Shared/Components/<Name>/Default.cshtml`，大小寫需為 `Components`；呼叫：`@await Component.InvokeAsync("<Name>")`。
- 範例：`CreatePostPopup`、`EditProfilePopup` 已集中至 `Views/Shared/Components/.../Default.cshtml`。
- Partial Views 建議放在 `Views/Shared/`，命名以底線開頭（如：`_FriendsListPartial.cshtml`）；使用：`<partial name="_FriendsListPartial" />` 或 `@await Html.PartialAsync("~/Views/Shared/_X.cshtml")`。
- 注意：Linux 伺服器大小寫敏感，請避免使用 `components`（小寫）資料夾。
