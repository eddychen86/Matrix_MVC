# Matrix 專案技術文件

> 基於 ASP.NET Core + Vue.js 的社群平台技術文件庫  
> **建立日期**: 2025-08-29  
> **最後更新**: 2025-08-31  

## 📋 文件概覽

本文件庫完整記錄 Matrix 專案使用的所有技術，從基礎到進階，包含使用說明、流程、原理與實際情境應用。

### 技術棧統計
- **後端技術**: 1 個主要文件
- **前端技術**: 2 個主要文件  
- **資料庫技術**: 1 個主要文件
- **DevOps**: 1 個主要文件
- **安全性**: 1 個主要文件
- **架構設計**: 1 個主要文件
- **總計**: 7 個技術文件

---

## 🗂️ 技術分類

### 🔧 後端開發 (Backend)
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`aspnet-core.md`](./Backend/aspnet-core.md) | ASP.NET Core 8.0 | 中級-高級 | Web 框架、MVC、API、中介軟體、DI 容器 |
| [`mvc-from-zero.md`](./Backend/mvc-from-zero.md) | ASP.NET Core MVC | 基礎 | 超新手友善，一步步完成 MVC 網站 |
| [`webapi-from-zero.md`](./Backend/webapi-from-zero.md) | ASP.NET Core Web API | 基礎 | 超新手友善，做出 Minimal/Controller API 與 CRUD |

**涵蓋範圍**: Web 應用程式開發、RESTful API、身份驗證、授權機制、中介軟體開發

---

### 🎨 前端開發 (Frontend)  
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`vue-integration.md`](./Frontend/vue-integration.md) | Vue.js 3.5 | 中級 | Vue + ASP.NET Core 整合、Composition API、模組化開發 |
| [`styling-system.md`](./Frontend/styling-system.md) | Tailwind CSS + DaisyUI | 基礎-中級 | 現代化樣式系統、響應式設計、組件化 UI |
| [`vue-webapi-integration.md`](./Frontend/vue-webapi-integration.md) | Vue + Web API | 基礎 | 用最少程式碼把 API 與 Vue 接起來 |

**涵蓋範圍**: 前端框架整合、樣式系統設計、響應式佈局、組件開發

---

### 💾 資料庫 (Database)
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`entity-framework.md`](./Database/entity-framework.md) | EF Core 8.0 | 中級-高級 | ORM、Code First、Migration、Repository 模式 |

**涵蓋範圍**: 資料存取層、資料庫設計、查詢優化、併發控制

---

### 🚀 DevOps 與部署 (DevOps)
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`deployment-config.md`](./DevOps/deployment-config.md) | Azure + Docker | 中級-高級 | 多環境部署、CI/CD、設定管理、監控 |
| [`deploy-from-zero.md`](./DevOps/deploy-from-zero.md) | 部署入門 | 基礎 | 從本機到雲端的最簡部署路線（含 Actions） |
| [`deploy-containers.md`](./DevOps/deploy-containers.md) | 容器化部署 | 基礎-中級 | 推送到 Docker Hub 並以 Azure Web App for Containers 運行 |

**涵蓋範圍**: 雲端部署、容器化、自動化部署、環境管理

---

### 🔐 安全性 (Security)
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`jwt-authentication.md`](./Security/jwt-authentication.md) | JWT + Cookie Auth | 中級-高級 | 無狀態認證、授權控制、安全性強化 |

**涵蓋範圍**: 身份驗證、授權機制、Token 管理、安全防護

---

### 🏗️ 架構設計 (Architecture)
| 文件 | 技術 | 複雜度 | 描述 |
|------|------|--------|------|
| [`signalr-realtime.md`](./Architecture/signalr-realtime.md) | SignalR Core | 中級-高級 | 即時通訊、雙向通信、連線管理、群組功能 |

**涵蓋範圍**: 即時通訊、WebSocket、推送通知、協作功能

---

## 🆕 近期更新 (2025-08-31)

- 後端修正: `CommonController` 移除 Primary Constructor 注入，於靜態方法透過 `HttpContext.RequestServices` 解析 `ILogger<CommonController>`，避免 CS9105/CS9113。
- 邊界處理: 修正使用者名稱裁切條件與長度不一致，避免 `Substring` 越界拋出 `ArgumentOutOfRangeException`。
- 模型擴充: `MenuViewModel` 新增 `DisplayName` 屬性並於選單顯示使用。
- 前端能力: 新增 Hook `wwwroot/js/hooks/useImgError.js`，整合列表頁處理圖片載入錯誤並替換為預設圖。
- 知識庫: `.QA_Book/ASP.NET-Core` 新增問題條目（Primary Constructor 日誌注入、DisplayName 大小寫、Substring 越界）並更新索引與統計。

文件新增：`Backend/mvc-from-zero.md`（從零到有的 MVC 教學，適合完全新手）

對應參考：`.QA_Book/ASP.NET-Core/common-logger-static.md`、`menu-auth-displayname.md`、`menu-substring-oob.md`


## 🎯 使用指南

### 新手入門路線
1. **後端基礎**: 先閱讀 [`aspnet-core.md`](./Backend/aspnet-core.md) 了解專案架構
2. **資料存取**: 接著學習 [`entity-framework.md`](./Database/entity-framework.md) 掌握資料操作
3. **前端整合**: 閱讀 [`vue-integration.md`](./Frontend/vue-integration.md) 了解前後端協作
4. **樣式系統**: 學習 [`styling-system.md`](./Frontend/styling-system.md) 掌握 UI 開發

### 進階開發路線
1. **安全機制**: 深入 [`jwt-authentication.md`](./Security/jwt-authentication.md) 學習認證授權
2. **即時功能**: 學習 [`signalr-realtime.md`](./Architecture/signalr-realtime.md) 實作即時通訊
3. **部署運維**: 掌握 [`deployment-config.md`](./DevOps/deployment-config.md) 完成專案部署

### 超新手路線圖（從零到上線）
1. 從零建立 MVC 專案 → [`mvc-from-zero.md`](./Backend/mvc-from-zero.md)
2. 最簡部署（Zip）到 Azure → [`deploy-from-zero.md`](./DevOps/deploy-from-zero.md)
3. 進階容器部署（Docker Hub → Azure）→ [`deploy-containers.md`](./DevOps/deploy-containers.md)
4. 若改走 Web API（非 MVC 畫面）→ [`webapi-from-zero.md`](./Backend/webapi-from-zero.md)
5. Web API + Vue 前端整合 → [`vue-webapi-integration.md`](./Frontend/vue-webapi-integration.md)

### 按技術領域查找
- **想了解後端開發** → Backend 分類
- **想學習前端技術** → Frontend 分類
- **想掌握資料庫** → Database 分類
- **想學習部署運維** → DevOps 分類
- **想加強安全性** → Security 分類
- **想實作即時功能** → Architecture 分類

---

## 📊 技術複雜度說明

| 等級 | 標準 | 說明 |
|------|------|------|
| **基礎** | 入門級 | 適合初學者，包含基本概念和使用方法 |
| **中級** | 實用級 | 需要一定基礎，涵蓋進階功能和最佳實務 |
| **高級** | 專家級 | 需要豐富經驗，包含複雜架構和優化技巧 |

---

## 🔍 文件特色

### ✅ 完整性
- **基礎到進階**: 每個技術都從基礎概念開始，逐步深入到進階應用
- **理論與實務**: 結合技術原理說明和實際程式碼範例
- **情境應用**: 提供真實的使用情境和解決方案

### ✅ 實用性
- **程式碼範例**: 所有範例都來自 Matrix 專案的實際程式碼
- **最佳實務**: 包含業界認可的最佳開發實務
- **效能優化**: 提供具體的效能優化建議和實作方法

### ✅ 結構化
- **統一格式**: 所有文件採用一致的結構和格式
- **清晰分類**: 按技術領域清楚分類，易於查找
- **相互參照**: 文件間相互參照，形成完整的知識體系

---

## 🎓 學習建議

### 時間分配建議
- **基礎學習**: 每個文件預估 2-3 小時深度閱讀
- **實作練習**: 每個技術建議投入 1-2 天實際操作
- **整合應用**: 建議花 1-2 週時間整合多項技術

### 學習順序建議
1. **依專案結構**: 從後端 → 資料庫 → 前端 → 安全 → 架構 → 部署
2. **依個人背景**: 根據自己的技術背景選擇起點
3. **依需求導向**: 根據當前工作需求選擇重點學習

### 實作建議
- **邊學邊做**: 建議在閱讀的同時搭配實際程式碼操作
- **筆記整理**: 建立個人的學習筆記和程式碼片段庫
- **問題追蹤**: 記錄學習過程中遇到的問題和解決方案

---

## 📝 文件維護

### 更新頻率
- **技術更新**: 當使用的技術版本更新時同步更新文件
- **內容完善**: 根據實際使用經驗持續完善內容
- **問題修正**: 發現錯誤時立即修正

### 貢獻指南
- **內容建議**: 歡迎提供內容改進建議
- **錯誤回報**: 發現文件錯誤請及時反映
- **經驗分享**: 歡迎分享實際使用經驗和最佳實務

---

## 📞 技術支援

### 學習資源
- **官方文檔**: 每個技術文件都提供官方文檔連結
- **範例程式**: 所有程式碼範例都可以在 Matrix 專案中找到
- **最佳實務**: 文件中包含業界認可的最佳開發實務

### 問題解決
- **常見問題**: 文件中包含常見問題的解決方案
- **故障排除**: 提供具體的問題診斷和解決步驟
- **效能調優**: 包含效能問題的分析和優化方法

---

**© 2025 Matrix Project Documentation. All rights reserved.**  
**Generated by Claude Code on 2025-08-29**
