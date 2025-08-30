# JavaScript 問題目錄

本目錄包含 JavaScript 相關的問題解決方案。

## 📋 問題清單

### 問題 1: PermissionService 不是函數錯誤
**檔案**: [`perm-err.md`](./perm-err.md)  
**描述**: 解決 JavaScript 服務物件被當作函數呼叫的錯誤  
**關鍵字**: PermissionService, 物件, 函數, 錯誤  
**相關檔案**: `config.js:274`

### 問題 2: JavaScript 解構賦值 null 物件錯誤
**檔案**: [`destruct-null.md`](./destruct-null.md)  
**描述**: 解決嘗試對 null 物件進行解構賦值時的 TypeError  
**關鍵字**: 解構賦值, destructuring, null, TypeError, 防護檢查  
**相關檔案**: `wwwroot/js/hooks/usePostActions.js:228-235`

### 問題 18: Vue 組件屬性跨頁面可用性問題
**檔案**: [`comp-prop.md`](./comp-prop.md)  
**描述**: 解決 Vue 組件在不同頁面使用時屬性未定義的問題  
**關鍵字**: Vue, 組件, 屬性, 跨頁面, 全域載入  
**相關檔案**: `main.js`, `home.js`, `*.cshtml`

### 問題 3: CKEditor 自訂樣式被覆蓋且 overflow 無效
**檔案**: [`ckeditor-style.md`](./ckeditor-style.md)  
**描述**: 解決 CKEditor 自訂樣式被預設樣式覆蓋及滾動控制問題  
**關鍵字**: CKEditor, CSS, 樣式覆蓋, overflow, 權重, !important  
**相關檔案**: Views/Shared/Components/CreatePostPopup/Default.cshtml

### 問題 4: 熱門文章輪播按鈕無法正確滾動到最後一個項目
**檔案**: [`carousel-scroll.md`](./carousel-scroll.md)  
**描述**: 解決輪播組件中 hotNext 按鈕無法讓最後一個項目居中顯示的問題  
**關鍵字**: 輪播, carousel, 滾動, 元素選擇器, getStep, getMaxIndex  
**相關檔案**: wwwroot/js/pages/home/home.js, Views/Home/Index.cshtml

---

## 🔍 快速搜尋

- **服務物件相關**: 問題 1
- **解構賦值相關**: 問題 2
- **Vue 組件相關**: 問題 18
- **CKEditor 相關**: 問題 3
- **輪播滾動相關**: 問題 4
- **函數調用錯誤**: 問題 1
- **屬性未定義**: 問題 18
- **TypeError**: 問題 2
- **CSS 樣式覆蓋**: 問題 3
- **元素選擇器錯誤**: 問題 4

## 📊 統計

- **總問題數**: 5
- **已解決**: 5
- **最後更新**: 2025-08-30