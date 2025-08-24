# 問題 19: Vue 編輯模式按鈕無法顯示問題

**症狀**: 在 `Areas/Dashboard/Views/Config/index.cshtml` 第211~218行，當 `item.isEditing === true` 時，編輯模式的按鈕無法顯示，只有空白的 template 標籤。

**原因**: HTML 模板中，當 `item.isEditing` 為 true 時對應的 template 區塊（191-193行和199-201行）是空的，沒有提供編輯狀態下應該顯示的 UI 元素。

**錯誤寫法**:
```html
<!-- 第191-193行 - SuperAdmin 欄位編輯狀態 -->
<template v-if="item.isEditing">
    <!-- 空白 - 沒有編輯UI -->
</template>

<!-- 第199-201行 - Status 欄位編輯狀態 -->  
<template v-if="item.isEditing">
    <!-- 空白 - 沒有編輯UI -->
</template>

<!-- 第211-218行 - 按鈕區域正常，有編輯按鈕 -->
<template v-if="item.isEditing">
    <button class="btn btn-ghost rounded-md btn-sm px-2 py-1" @@click="saveAdmin(item.id)">
        <i data-lucide="save" class="size-6 text-blue-500 cursor-pointer"></i>
    </button>
    <button class="btn btn-ghost rounded-md btn-sm px-2 py-1" @@click="cancelEdit(item.id)">
        <i data-lucide="ban" class="size-6 text-red-500 cursor-pointer"></i>
    </button>
</template>
```

**正確寫法**:
```html
<!-- 第191-193行 - SuperAdmin 欄位編輯狀態 -->
<template v-if="item.isEditing">
    <select class="select select-primary select-sm w-full max-w-xs"
            v-model="item.SuperAdmin">
        <option value="1">一般管理員</option>
        <option value="2">超級管理員</option>
    </select>
</template>

<!-- 第199-201行 - Status 欄位編輯狀態 -->
<template v-if="item.isEditing">
    <select class="select select-primary select-sm w-full max-w-xs"
            v-model="item.status">
        <option value="0">停用</option>
        <option value="1">啟用</option>
    </select>
</template>

<!-- 第211-218行保持不變 - 按鈕區域已經正常 -->
```

**完整解決方案**:

需要在空白的 template 區塊中加入對應的編輯控制項：

1. **SuperAdmin 欄位編輯**: 提供下拉選單讓使用者選擇管理員級別
2. **Status 欄位編輯**: 提供下拉選單讓使用者切換啟用/停用狀態
3. **資料同步**: 確保編輯的值能正確綁定到 `item` 物件的對應屬性

**JavaScript 支援檢查**:
- `editAdmin()` 函數已正確設定 `isEditing = true` 
- `saveAdmin()` 函數需要讀取編輯後的值並發送到後端API
- `cancelEdit()` 函數需要恢復原始資料並設定 `isEditing = false`

**相關檔案**: 
- `/Areas/Dashboard/Views/Config/index.cshtml:191-193, 199-201`
- `/wwwroot/js/dashboard/pages/config/config.js:311-396`

**解決後的效果**:
- 點擊編輯按鈕後，SuperAdmin 和 Status 欄位會變成可編輯的下拉選單
- 點擊儲存按鈕會將修改結果傳送到後端API
- 點擊取消按鈕會恢復原始資料並退出編輯模式