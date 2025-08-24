# Vue.js 問題目錄

本目錄包含 Vue.js 框架相關的問題解決方案。

## 📋 問題清單

### 問題 6: Vue 3 響應式物件重置錯誤
**檔案**: [`react-rst.md`](./react-rst.md)  
**描述**: 解決 Vue 3 reactive() 物件重置時失去響應式的問題  
**關鍵字**: Vue3, reactive, ref, 響應式, 重置  
**相關檔案**: `config.js:135-145`

### 問題 7: Vue 元件屬性未定義錯誤
**檔案**: [`prop-undef.md`](./prop-undef.md)  
**描述**: 解決 Vue 組件屬性在渲染時未定義的錯誤  
**關鍵字**: Vue, 屬性, undefined, render, 組件  
**相關檔案**: `reply.js`, `main.js:157`, `Index.cshtml:88-90`

### 問題 11: Vue v-model 使用可選鏈結運算子導致 Invalid left-hand side in assignment
**檔案**: [`vmodel-opt.md`](./vmodel-opt.md)  
**描述**: 解決 v-model 與可選鏈結運算子衝突的語法錯誤  
**關鍵字**: Vue, v-model, 可選鏈結, ?., 語法錯誤  
**相關檔案**: `Config/index.cshtml:27-43`, `config.js`

### 問題 12: 配置管理頁面的完整功能實作與 v-for 優化
**檔案**: [`config-vfor.md`](./config-vfor.md)  
**描述**: 使用 v-for 優化配置管理頁面的重複程式碼  
**關鍵字**: Vue, v-for, 配置管理, CRUD, 分頁  
**相關檔案**: `config.js:21-24`, `Config/index.cshtml:25-71`

### 問題 18: Vue 組件屬性跨頁面可用性問題
**檔案**: [`cross-page.md`](./cross-page.md)  
**描述**: 解決 Vue 組件在不同頁面使用時屬性可用性問題  
**關鍵字**: Vue, 組件, 跨頁面, 全域載入, 屬性共享  
**相關檔案**: `main.js:7,196,319`, `home.js:1,127,130`

---

## 🔍 快速搜尋

### 難度分級
- **初級**: 問題 7 (屬性未定義)
- **中級**: 問題 6 (響應式重置), 問題 11 (v-model 語法)
- **高級**: 問題 12 (v-for 優化), 問題 18 (跨頁面組件)

### 主題分類
- **響應式系統**: 問題 6
- **組件系統**: 問題 7, 18
- **模板語法**: 問題 11, 12
- **架構設計**: 問題 18

## 📊 統計

- **總問題數**: 5
- **已解決**: 5
- **Vue 2 相關**: 0
- **Vue 3 相關**: 5
- **最後更新**: 2025-08-25