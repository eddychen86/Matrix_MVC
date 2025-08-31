# 後端技術文件索引

**分類**: Backend Development  
**技術領域**: 伺服器端開發、API 設計、業務邏輯  

## 📋 文件列表

### 文件 1: ASP.NET Core 完整技術指南
**檔案**: [`aspnet-core.md`](./aspnet-core.md)  
**描述**: ASP.NET Core 8.0 完整開發指南，涵蓋 MVC、API、中介軟體、依賴注入等核心技術  
**關鍵字**: ASP.NET Core, MVC, Web API, Middleware, Dependency Injection, Authentication, Authorization  
**相關檔案**: Program.cs, Controllers/, Services/, Middleware/  
**複雜度**: 中級到高級  

**內容概要**:
- 專案架構與組織
- 服務註冊與依賴注入
- 中介軟體鏈設計
- Repository Pattern 實作
- API 開發與設計
- 身份驗證與授權
- 效能優化技術

---

### 文件 2: 從零開始建立 MVC 專案（超新手）
**檔案**: [`mvc-from-zero.md`](./mvc-from-zero.md)  
**描述**: 超新手友善，逐步帶你用 .NET CLI 建立 MVC、認識資料夾結構、加控制器與 View、加入 Model 與 SQLite、做出簡單 CRUD  ️ 
**關鍵字**: 入門, MVC, .NET CLI, Controller, View, Model, EF Core, SQLite  
**相關檔案**: Controllers/, Views/, Models/, Data/, Program.cs  
**複雜度**: 基礎  

**內容概要**:
- 建立專案與啟動網站
- 建立控制器與簡單頁面
- 建立 Model 與強型別 View
- EF Core + SQLite + Migration
- 簡易 CRUD（文章列表與新增）
- 常見錯誤排查（大小寫、Substring、DI、Razor 路徑）

---

### 文件 3: 從零開始建立 Web API（超新手）
**檔案**: [`webapi-from-zero.md`](./webapi-from-zero.md)  
**描述**: 不用 MVC 畫面，直接做出 Minimal API/Controller API、Swagger 測試、加上 SQLite/EF Core 做 CRUD，並設定 CORS 提供給前端  
**關鍵字**: 入門, Web API, Minimal API, Controller, Swagger, EF Core, SQLite, CORS  
**相關檔案**: Program.cs, Controllers/, Models/, Data/  
**複雜度**: 基礎  

**內容概要**:
- 使用 `dotnet new webapi` 快速建立
- Minimal API 與 Controller 兩種路線
- 加入 Model、In-Memory/SQLite 資料儲存
- 使用 Swagger 測試 API
- 啟用 CORS 給前端呼叫

---

## 🎯 學習路線

### 入門階段 (1-2 週)
1. **基礎概念**: 理解 ASP.NET Core 的核心概念
2. **專案結構**: 熟悉 Matrix 專案的組織架構
3. **依賴注入**: 掌握 DI 容器的使用方法

### 進階階段 (2-3 週)  
1. **中介軟體開發**: 學習自定義中介軟體
2. **API 設計**: 掌握 RESTful API 最佳實務
3. **Repository 模式**: 理解資料存取層抽象

### 專家階段 (1-2 週)
1. **效能優化**: 學習快取、非同步等優化技術
2. **安全強化**: 實作進階安全機制
3. **架構設計**: 掌握大型專案的架構設計

---

## 🔗 技術關聯

### 直接相關技術
- **Entity Framework Core**: 資料存取層 ORM 技術
- **JWT Authentication**: 身份驗證機制
- **SignalR**: 即時通訊功能

### 整合技術
- **Vue.js**: 前端框架整合
- **Docker**: 容器化部署
- **Azure**: 雲端服務整合

---

## 📚 推薦閱讀順序

1. **先決條件**: 具備 C# 基礎和 Web 開發概念
2. **配合閱讀**: 同時參考 Entity Framework 文件了解資料存取
3. **實作練習**: 建議搭配 Matrix 專案程式碼進行實作
4. **進階學習**: 完成後可深入學習 SignalR 和安全機制

---

**最後更新**: 2025-08-31  
**文件數量**: 3  
**總學習時間**: 基礎 1-2 天；進階 4-7 週 (依個人基礎而定)
