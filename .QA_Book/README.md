# Matrix 專案 QA 知識庫

結構化的問題解答知識庫，由原 `Learning.md` 重新組織而成。**純 Markdown 格式**，便於跨平台閱讀和維護。

## 📁 資料夾結構

```
.QA_Book/
├── JavaScript/           # JavaScript 相關問題
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   ├── perm-err.md      # 問題 1: PermissionService 錯誤
│   └── comp-prop.md     # 問題 18: 組件屬性跨頁面問題
├── Vue/                 # Vue.js 框架相關問題
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   ├── react-rst.md     # 問題 6: 響應式重置錯誤
│   ├── vmodel-opt.md    # 問題 11: v-model 語法錯誤
│   └── ...              # 其他 Vue 相關問題
├── ASP.NET-Core/        # ASP.NET Core 後端相關問題
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   └── ...              # ASP.NET Core 相關問題檔案
├── Git/                 # Git 版本控制相關問題
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   └── ...              # Git 相關問題檔案
├── Architecture/        # 系統架構、中介軟體相關問題
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   └── ...              # 架構相關問題檔案
├── General/             # 通用程式設計原則
│   ├── index.md         # 分類目錄 (可點擊跳轉)
│   └── principles.md    # 程式設計原則與洞察
└── README.md           # 本文件
```

## 🔍 快速導航

### JavaScript (6 個問題)
- 問題 1: PermissionService 不是函數錯誤 → `perm-err.md`
- 問題 2: JavaScript 解構賦值 null 物件錯誤 → `destruct-null.md`
- 問題 3: CKEditor 自訂樣式被覆蓋且 overflow 無效 → `ckeditor-style.md`
- 問題 4: 熱門文章輪播按鈕無法正確滾動到最後一個項目 → `carousel-scroll.md`
- 問題 5: 函數式編程風格的圖片錯誤處理 Hook → `fp-img-error.md`
- 問題 18: Vue 組件屬性跨頁面可用性問題 → `comp-prop.md`

### Vue.js (7 個問題)
- 問題 6: Vue 3 響應式物件重置錯誤 → `react-rst.md`
- 問題 7: Vue 元件屬性未定義錯誤 → `prop-undef.md`
- 問題 11: Vue v-model 使用可選鏈結運算子導致錯誤 → `vmodel-opt.md`
- 問題 12: 配置管理頁面 v-for 優化 → `config-vfor.md`
- 問題 18: 組件屬性跨頁面可用性 → `cross-page.md`
- 問題 23: CKEditor Vue 3 生命週期函數調用錯誤 → `ckeditor-lifecycle.md`
- 問題 24: 圖片載入錯誤處理與動態顯示切換 → `img-error.md`

### ASP.NET Core (21 個問題)
- 問題 2: Claims 資訊調試 → `claims-dbg.md`
- 問題 3: 篩選特定 Claims → `claims-flt.md`
- 問題 8: 管理員註冊服務實作 → `admin-reg.md`
- 問題 9: AdminRegistrationService 編譯錯誤 → `admin-cmp.md`
- 問題 10: 介面實作錯誤修復 → `admin-int.md`
- 問題 13: ConfigController TODO 實作 → `ctrl-todo.md`
- 問題 14: 後端篩選 API 實作 → `api-filt.md`
- 問題 15: Profile 頁面載入狀態管理 → `prof-load.md`
- 問題 19: 基於 Token 的忘記密碼功能 → `pwd-reset.md`
- 問題 20: 未登入用戶訪問頁面自動重定向 → `auth-redirect.md`
- 問題 21: macOS 上的 NuGet 包路徑解析錯誤 → `nuget-path.md`
- 問題 22: ViewComponent 中引用使用者資訊 → `vw-comp-usr.md`
- 問題 24: 忘記密碼後修改密碼導致登入失敗 → `pwd-change-fix.md`
- 問題 25: 忘記密碼流程 Token 管理 → `forgot-password-flow.md`
- 問題 26: Razor 頁面中 Vue.js @error 事件語法編譯錯誤 → `razor-vue-error.md`
- 問題 27: 共用圖片驗證服務與數據清理 → `img-validation-service.md`
- 問題 28: EnrichWithImageValidationAsync 可空性 CS8603 修復 → `nullable-cs8603-imgvalidation.md`
- 問題 29: ValidateImagesAsync 重複鍵導致 ToDictionary 例外 (500) → `img-validation-dupkey.md`
- 問題 30: CommonController Primary Constructor 注入 ILogger 造成 CS9105/CS9113 → `common-logger-static.md`
- 問題 31: CommonController 指向不存在的屬性導致 CS1061（auth.displayName） → `menu-auth-displayname.md`
- 問題 32: 使用者名稱 Substring 超出長度導致 ArgumentOutOfRangeException → `menu-substring-oob.md`

### Git 版本控制 (2 個問題)
- 問題 4: 搜尋所有分支中的關鍵字 → `git-srch.md`
- 問題 5: 從舊 commit 選擇性合併檔案 → `git-file.md`

### 系統架構 (3 個問題)
- 問題 13: 多語言 placeholder 整合 → `i18n-plh.md`
- 問題 16: AdminActivityService 活動記錄整合 → `admin-act.md`
- 問題 17: AdminActivityMiddleware nullable 警告修復 → `mid-null.md`

### 通用原則 (1 個問題)
- 程式設計原則與洞察 → `principles.md`

## 🛠️ 使用方式

### 1. 透過分類瀏覽 (推薦)
1. 點擊上方**快速導航**中的連結，或直接進入對應資料夾
2. 查看各分類的 `index.md` 目錄檔案
3. 點擊具體問題的連結跳轉到解決方案

### 2. 直接搜尋問題檔案
根據短檔名快速定位：
- `perm-err.md` - PermissionService 錯誤
- `react-rst.md` - Vue 響應式重置
- `vmodel-opt.md` - v-model 語法問題
- 等等...

### 3. 使用 IDE/編輯器搜尋
在整個 `.QA_Book/` 資料夾中搜尋關鍵字，快速找到相關問題。

## 📊 統計資訊

- **總問題數量**: 37 個
- **程式語言**: JavaScript, C#
- **框架技術**: Vue.js, ASP.NET Core
- **工具系統**: Git
- **最後更新**: 2025-08-31
- **版本**: v2.10 (新增 ValidateImagesAsync 去重修復)

## 🔄 從原版本遷移

原 `Learning.md` 的所有內容都已重新組織到對應的分類資料夾中，每個問題都有獨立的檔案和唯一的短雜湊檔名，便於快速查找和引用。

## 📝 貢獻指引

新問題應該：
1. 放在適當的分類資料夾中
2. 使用 8 字元的簡短檔名
3. 更新對應資料夾的 `index.js`
4. 更新主目錄的 `mapping.js`
