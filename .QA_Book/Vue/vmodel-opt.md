# 問題 11: Vue v-model 使用可選鏈結運算子導致 Invalid left-hand side in assignment

**症狀**: 在瀏覽器 console 出現「Uncaught SyntaxError: Invalid left-hand side in assignment」，發生於 `Areas/Dashboard/Views/Config/index.cshtml` 第27–43行（`v-model` 綁定處）。

**原因**: `v-model` 會被 Vue 編譯為賦值表達式，例如 `model[key] = $event`。若在 `v-model` 的左側使用可選鏈結運算子（如 `obj?.[key]`），編譯後會變成 `obj?.[key] = ...`，這在 JavaScript 中是無效的賦值左值，因此觸發語法錯誤。

**錯誤寫法**:
```html
<input v-model="adminFilterVal?.[tool.title]">
<input type="checkbox" v-model="adminFilterVal?.[tool.title]">
```

**正確寫法**:
```html
<input v-model="adminFilterVal[tool.title]">
<input type="checkbox" v-model="adminFilterVal[tool.title]">
```

**解決方案**: 
- 將 `v-model` 左側的可選鏈結 `?.` 移除，確保是可指派的目標。
- 確保 `adminFilterVal` 於初始化時為物件（例如 `ref({})` 或含預設鍵值的物件），避免取值時為 `null/undefined`。
- 若擔心鍵不存在，可在初始化時先建立對應鍵（例如 `Keyword/SuperAdmin/Status`）。
- 檢查其他相似寫法（如被註解的 `logFilterVal?.[tool.title]`）並一併改為不含 `?.`。

**相關檔案**: 
- `/Areas/Dashboard/Views/Config/index.cshtml:27-43, 240-256`
- `/wwwroot/js/dashboard/pages/config/config.js`（`adminFilterVal` 初始化位置）