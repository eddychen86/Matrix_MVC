# Matrix JavaScript 模組化架構

這個文件展示了 Matrix 專案 JavaScript 代碼的模組化重構結果。

## 📁 資料夾結構

```
wwwroot/js/
├── utils/                  # 工具函數
│   ├── formatting.js       # 日期格式化和時間相對顯示
│   ├── api.js              # API 請求工具
│   └── dom.js              # DOM 操作工具
├── core/                   # 核心管理器
│   ├── auth-manager.js     # 認證狀態管理
│   ├── popup-manager.js    # 登入彈窗管理
│   └── language-manager.js # 多語言管理
├── hooks/                  # 可重用邏輯 Hook
│   ├── usePasswordToggle.js   # 密碼顯示切換
│   ├── useFormValidation.js   # 表單驗證
│   └── useAuthForm.js         # 認證表單通用邏輯
├── components/             # Vue 組件
│   ├── main-app.js         # 主應用程式組件
│   └── auth-forms.js       # 認證表單組件
├── pages/                  # 頁面專用邏輯
│   ├── home.js             # 首頁功能
│   └── error.js            # 錯誤頁面處理
├── main-new.js             # 重構後的主入口點
├── main.js                 # 原始文件（保留參考）
└── README.md               # 本說明文件
```

## 🚀 使用方式

### 在 .cshtml 文件中引用

按照依賴順序引用這些 JavaScript 文件：

```html
<!-- 基礎庫 -->
<script src="~/lib/vue/dist/vue.global.js"></script>
<script src="~/lib/lucide/dist/umd/lucide.min.js"></script>

<!-- 工具函數 (無依賴) -->
<script src="~/js/utils/formatting.js"></script>
<script src="~/js/utils/dom.js"></script>
<script src="~/js/utils/api.js"></script>

<!-- Hooks (依賴 utils) -->
<script src="~/js/hooks/usePasswordToggle.js"></script>
<script src="~/js/hooks/useFormValidation.js"></script>
<script src="~/js/hooks/useAuthForm.js"></script>

<!-- 核心管理器 (依賴 utils) -->
<script src="~/js/core/auth-manager.js"></script>
<script src="~/js/core/popup-manager.js"></script>
<script src="~/js/core/language-manager.js"></script>

<!-- 組件 (依賴 hooks 和 core) -->
<script src="~/js/components/main-app.js"></script>
<script src="~/js/components/auth-forms.js"></script>

<!-- 頁面邏輯 (依賴 components) -->
<script src="~/js/pages/home.js"></script>
<script src="~/js/pages/error.js"></script>

<!-- 主入口點 (最後載入) -->
<script src="~/js/main-new.js"></script>
```

## 📋 重構前後對比

### 重構前 (main.js)
- ❌ 單一文件 870+ 行
- ❌ 功能混雜難以維護
- ❌ 大量重複代碼
- ❌ 沒有明確的依賴關係

### 重構後
- ✅ 模組化架構，職責分離
- ✅ 可重用的 Hook 和工具函數
- ✅ 清晰的依賴關係
- ✅ 易於測試和維護
- ✅ 支援按需載入

## 🔧 主要改進

### 1. 消除重複代碼
- **密碼切換功能**: 登入和註冊頁面共用 `usePasswordToggle`
- **表單驗證**: 統一使用 `useFormValidation`
- **API 請求**: 統一使用 `useApi`
- **DOM 操作**: 統一使用 `useDom`

### 2. 增強可維護性
- 每個模組職責單一
- 依賴關係清晰
- 易於單元測試

### 3. 提升擴展性
- 新功能可以獨立開發
- 模組可以獨立升級
- 支援懶載入

## 💡 各模組說明

### Utils (工具函數)
- **formatting.js**: 日期時間格式化
- **api.js**: 統一的 API 請求方法
- **dom.js**: DOM 操作和選擇器

### Core (核心管理器)
- **auth-manager.js**: 處理認證狀態和登出
- **popup-manager.js**: 管理訪客登入提示彈窗
- **language-manager.js**: 多語言切換功能

### Hooks (可重用邏輯)
- **usePasswordToggle.js**: 密碼顯示/隱藏切換
- **useFormValidation.js**: 表單驗證和錯誤處理
- **useAuthForm.js**: 登入註冊表單通用邏輯

### Components (Vue 組件)
- **main-app.js**: 主應用的 Vue 組件邏輯
- **auth-forms.js**: 認證表單的 Vue 組件

### Pages (頁面專用)
- **home.js**: 首頁特定功能 (無限滾動、文章互動)
- **error.js**: 錯誤頁面處理

## 🔄 遷移步驟

1. **測試現有功能**: 確保原始 `main.js` 運作正常
2. **逐步引入**: 先引入 utils 模組
3. **替換組件**: 逐一替換各個功能模組
4. **測試驗證**: 每個模組替換後進行測試
5. **完全遷移**: 最後使用 `main-new.js` 替換 `main.js`

## 🎯 使用建議

1. **開發新功能時**: 優先考慮是否可以重用現有的 Hook 或工具
2. **修改現有功能**: 在對應的模組中進行修改
3. **添加新頁面**: 在 `pages/` 目錄下創建新的頁面邏輯文件
4. **全域功能**: 添加到 `core/` 目錄下的管理器中

## ⚠️ 注意事項

1. **依賴順序**: 必須按照正確的順序載入 JavaScript 文件
2. **全域變數**: 各模組會將功能掛載到 `window` 對象上
3. **向後兼容**: 保留了原有的全域方法供現有代碼使用
4. **Lucide 圖標**: 需要在 DOM 變更後調用 `lucide.createIcons()`