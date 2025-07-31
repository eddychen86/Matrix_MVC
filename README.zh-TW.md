以英文閱讀: [English](README.md)

# Matrix
---

## 總覽
Matrix 是一個為 Web3 先驅和深度技術愛好者打造的庇護所，旨在過濾主流社交媒體的噪音。我們提供一個純粹、專注的環境，進行高品質的對話。在這裡，鏈上憑證是唯一的通行證，確保社群建立在專業知識和共享共識之上。雖然平台最初可能充滿零散的見解，但我們相信，隨著真正的同行相互連結，這些無序的微光將形成一個宏大而有序的思想矩陣。

## 工具
![ASP.NET](https://img.shields.io/badge/ASP.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white) ![DaisyUI](https://img.shields.io/badge/daisyui-5A0EF8?style=for-the-badge&logo=daisyui&logoColor=white) ![SCSS](https://img.shields.io/badge/SCSS-CC6699?style=for-the-badge&logo=sass&logoColor=white)


## 步驟
首先，如果您沒有或不使用 Visual Studio，您需要安裝 libman，請在您的終端機中輸入此命令列：
```
dotnet tool install Microsoft.Web.LibraryManager.Cli
```
安裝 libman 後，您需要將相依性套件安裝到 `wwwroot/lib` 資料夾中。
```
dotnet tool run libman restore
```
然後，您還需要安裝這些套件和工具。
<i><b>如果您使用的是 Visual Studio，您可以在 Nuget 擴充功能管理中安裝它。</b></i>
```
dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 8.0.11
```
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

  2. 取得 daisyUI 捆綁的 JS 檔案 (已有)
  3. 監看 CSS
      當您執行以下命令時，"tailwindcss" 將在背景中被監聽。
      ###### MacOS
      ```
      ./tw.sh
      ```
      ###### windows
      ```
      .\tw.bat
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
