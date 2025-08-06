以英文閱讀: [English](README.md)

# Matrix
---

## 總覽
Matrix 是一個為 Web3 先驅和深度技術愛好者打造的庇護所，旨在過濾主流社交媒體的噪音。我們提供一個純粹、專注的環境，進行高品質的對話。在這裡，鏈上憑證是唯一的通行證，確保社群建立在專業知識和共享共識之上。雖然平台最初可能充滿零散的見解，但我們相信，隨著真正的同行相互連結，這些無序的微光將形成一個宏大而有序的思想矩陣。

## 工具
![ASP.NET](https://img.shields.io/badge/ASP.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white) ![DaisyUI](https://img.shields.io/badge/daisyui-5A0EF8?style=for-the-badge&logo=daisyui&logoColor=white) ![SCSS](https://img.shields.io/badge/SCSS-CC6699?style=for-the-badge&logo=sass&logoColor=white)


## 步驟
1. 請先安裝 NodeJS
2. 如果您沒有或不使用 Visual Studio，您需要安裝 libman，請在您的終端機中輸入此命令列：
    ```
    dotnet tool install Microsoft.Web.LibraryManager.Cli
    npm i -g sass
    ```
3. 安裝 libman 後，您需要將相依性套件安裝到 `wwwroot/lib` 資料夾中。
    ```
    dotnet tool run libman restore
    ```
4. 您還需要安裝這些套件和工具。
    <i><b>如果您使用的是 Visual Studio，您可以在 Nuget 擴充功能管理中安裝它。</b></i>
    ```
    dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 8.0.11
    dotnet add package MailKit
    dotnet add package AutoMapper
    ```
<br />
因為這個專案使用了 DaisyUI UI 函式庫，所以您需要安裝 tailwindcss CLI 和 DaisyUI.<br>

  1. 取得 Tailwind CSS 執行檔
  遵循 Tailwind CSS 指南，取得適用於您作業系統的最新版本的 Tailwind CSS 執行檔。

      ###### windows
      ```
      curl -sLo tailwindcss.exe https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
      ```
      ###### MacOS
      ```
      curl -sLo tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-arm64
      ```
      使檔案可執行 (適用於 Linux 和 MacOS)：`chmod +x tailwindcss`

  1. 取得 daisyUI 捆綁的 JS 檔案 (已有)
  2. 監看 CSS
      當您執行以下命令時，"tailwindcss" 將在背景中被監聽。
      ###### MacOS
      ```
      sass "wwwroot/scss/main.scss" "wwwroot/css/components.css" -w --no-source-map
      ./tailwindcss -i "wwwroot/css/tailwind.css" -o "wwwroot/css/site.css" -w
      ```
      ###### windows
      ```
      sass ".\wwwroot\scss\main.scss" ".\wwwroot\css\components.css" -w --no-source-map
      .\tailwindcss.exe -i ".\wwwroot\css\tailwind.css" -o ".\wwwroot\css\site.css" -w
      ```

---
# 附註

檢查所有 EF 工具資訊
```
dotnet list package
dotnet new view -n <cshtml name> -o <target folder>      # 建立新視圖
```

---
## 架構說明

本專案遵循分層架構模式，主要使用以下資料夾來組織程式碼：

*   `Controllers` (控制器)
*   `Models` (模型)
*   `Data/Configurations` (資料設定)
*   `DTOs` (資料傳輸物件)
*   `Services` (服務)

以下是它們的角色和關係的說明：

### 1. `Controllers` (控制器)

*   **角色**：**控制器**作為傳入請求 (例如，來自網頁瀏覽器或 API 用戶端) 的進入點。它們負責處理使用者輸入並協調回應。
*   **功能**：控制器接收請求，呼叫適當的 `Service` 來執行必要的業務邏輯，然後傳回適當的回應 (例如，網頁、JSON 資料或重新導向)。
*   **關鍵原則**：控制器應該是輕量級的。它們不應包含業務邏輯或直接與資料庫互動。它們的主要作用是協調視圖/用戶端和服務層之間的互動。

### 2. `Models` (模型)

*   **角色**：**模型**是您應用程式中資料的「藍圖」。它們定義了您在 C# 世界中資料的形狀和關係。
*   **功能**：模型類別 (例如 `Article.cs`) 定義了一個實體的屬性以及它與其他實體的關係。它就像是資料庫資料表的 C# 表示法。

### 3. `Data/Configurations` (資料設定)

*   **角色**：此層作為資料庫的「施工說明書」。它定義了 `Models` 如何對應到實際的資料庫資料表。
*   **功能**：這些檔案使用 Fluent API 為 Entity Framework Core 提供如何建立資料庫結構的詳細指令。這包括設定主鍵、定義欄位屬性 (如最大長度)、建立索引和設定預設值。

### 4. `DTOs` (資料傳輸物件)

*   **角色**：DTO 作為應用程式不同層之間傳輸資料的「契約」或定義好的形狀，特別是在 **控制器 (Controllers)** 和 **服務 (Services)** 之間。
*   **功能**：它們是包含屬性但沒有業務邏輯的簡單類別。它們用於：
    *   接收來自傳入請求的資料 (例如，從 Web 表單或 API 呼叫)。
    *   在回應中傳回資料。
    *   使用資料註解 (Data Annotations) 強制執行驗證規則 (例如 `[Required]`, `[StringLength]`)。

### 5. `Services` (服務)

*   **角色**：**服務 (Services)** 層是應用程式的「大腦」。它包含核心業務邏輯。
*   **功能**：服務類別從控制器接收一個 DTO，處理它，並執行必要的動作。這可以包括：
    *   根據業務規則驗證資料。
    *   與資料庫互動 (透過 `DbContext`)。
    *   呼叫其他服務。
    *   將資料從 DTO 對應到資料庫模型 (實體)，反之亦然。

### 關係流程 (範例：建立文章)

1.  使用者透過 UI 提交新文章。請求到達 **控制器 (Controller)**。
2.  **控制器** 接收請求資料並將其繫結到 `CreateArticleDto`。
3.  **控制器** 將 `CreateArticleDto` 傳遞給 `ArticleService`。
4.  `ArticleService` 驗證 DTO，建立一個 `Article` 模型執行個體，並使用 DTO 的資料填入它。
5.  `ArticleService` 使用 `DbContext` 來儲存 `Article` 模型。
6.  Entity Framework Core 讀取 `ArticleConfiguration` 以了解如何建立 SQL `INSERT` 陳述式，並將資料寫入資料庫。

這種關注點分離使得應用程式更容易維護、測試和擴展。

---

## Code First 規範與最佳實務

本專案遵循 **Entity Framework Core Code First** 方法，這意味著資料庫結構是從程式碼 (Models) 生成的。為了維持程式碼完整性並避免同步問題，請遵循以下指導原則：

### 🚨 **絕對不要** 直接修改資料庫！

進行資料庫變更時，**務必** 使用 Code First 工作流程：

### 正確的資料庫變更工作流程

1. **先修改 Model** 在 `/Models/` 資料夾中
2. **更新 Configuration** 如有需要，在 `/Data/Configurations/` 中
3. **建立 Migration** 使用 EF Core 工具
4. **套用 Migration** 來更新資料庫

### 必要指令

```bash
# 在模型變更後新增 migration
dotnet ef migrations add <MigrationName>

# 套用待處理的 migration 到資料庫
dotnet ef database update

# 移除最後一個 migration (如果尚未套用)
dotnet ef migrations remove

# 檢查 migration 狀態
dotnet ef migrations list

# 從 migration 生成 SQL 腳本
dotnet ef migrations script
```

### 逐步範例：新增新欄位

1. **將欄位新增到您的 Model：**
   ```csharp
   // 在 Models/Person.cs 中
   [MaxLength(100)]
   public string? NewField { get; set; }
   ```

2. **更新 Configuration (如有需要)：**
   ```csharp
   // 在 Data/Configurations/PersonConfiguration.cs 中
   builder.Property(p => p.NewField)
       .HasMaxLength(100);
   ```

3. **建立 Migration：**
   ```bash
   dotnet ef migrations add AddNewFieldToPerson
   ```

4. **套用 Migration：**
   ```bash
   dotnet ef database update
   ```

### 常見情況與解決方案

#### 🔄 **如果有人意外直接修改了資料庫：**

1. 手動將缺失的欄位新增到適當的 Model
2. 如有需要，更新 Configuration
3. **從資料庫中刪除手動新增的欄位**
4. 建立新的 migration：`dotnet ef migrations add RestoreCodeFirstIntegrity`
5. 套用 migration：`dotnet ef database update`

#### 🔍 **檢查同步問題：**
```bash
# 如果一切都同步，這將建立一個空的 migration
dotnet ef migrations add CheckSync

# 如果 migration 是空的，請移除它
dotnet ef migrations remove
```

#### 📝 **Migration 命名慣例：**
- 使用描述性名稱：`AddUserEmailField`、`UpdateArticleConstraints`
- 使用 PascalCase 格式
- 包含動作和受影響的實體

### 資料庫連線

專案使用 Entity Framework Core 搭配 SQL Server。連線字串應該在以下位置設定：
- `appsettings.json` 用於正式環境
- `appsettings.Development.json` 用於開發環境
- 或使用 `.env` 檔案 (搭配 DotNetEnv 套件)

### ⚠️ 重要注意事項

- **絕對不要** 對資料庫執行直接的 SQL 命令來變更結構
- **務必** 先在開發資料庫上測試 migration
- 在正式環境套用 migration 之前，**務必備份** 您的資料庫
- 套用前**務必檢查** 生成的 migration 程式碼
- **務必將** migration 檔案保存在版本控制中

遵循這些指導原則可確保您的資料庫結構與程式碼保持同步，並防止資料遺失或損壞問題。
