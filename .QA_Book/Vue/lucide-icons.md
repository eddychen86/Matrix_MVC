# 問題 22: Lucide Icons 無法顯示問題

**症狀**: 在某些頁面中，Lucide icons 使用 `data-lucide` 屬性但無法正常顯示

**原因**: 缺少 `lucide.createIcons()` 的呼叫來初始化 icons

**問題分析**:
- Lucide library 已載入（`~/lib/lucide/dist/umd/lucide.min.js`）
- HTML 中正確使用了 `data-lucide="icon-name"` 屬性
- 但缺少 `lucide.createIcons()` 呼叫來將 `data-lucide` 屬性轉換為實際的 SVG icons

**錯誤寫法**:
```html
<!-- 只有 HTML，沒有 JavaScript 初始化 -->
<i data-lucide="mail" class="team-icon"></i>
<i data-lucide="github" class="team-icon"></i>
```

**正確寫法**:
```html
<!-- HTML -->
<i data-lucide="mail" class="team-icon"></i>
<i data-lucide="github" class="team-icon"></i>

<!-- JavaScript 初始化 -->
@section Scripts {
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            if (typeof lucide !== 'undefined' && lucide.createIcons) {
                lucide.createIcons();
            }
        });
    </script>
}
```

**解決方案 1: 在頁面中直接初始化**:
```html
@section Scripts {
    <script>
        // 初始化 Lucide icons
        document.addEventListener('DOMContentLoaded', function() {
            if (typeof lucide !== 'undefined' && lucide.createIcons) {
                lucide.createIcons();
            }
        });
    </script>
}
```

**解決方案 2: 在 Vue 組件中初始化**:
```javascript
// 在 onMounted 生命週期中
onMounted(() => {
    // 其他初始化代碼...
    
    // 初始化 Lucide icons
    if (typeof lucide !== 'undefined' && lucide.createIcons) {
        lucide.createIcons()
    }
})
```

**解決方案 3: 動態內容更新後重新初始化**:
```javascript
// 在動態添加內容後
function updateContent() {
    // 更新 DOM 內容...
    
    // 重新初始化 icons
    if (window.lucide && window.lucide.createIcons) {
        setTimeout(() => window.lucide.createIcons(), 0)
    }
}
```

**最佳實務**:
1. 在頁面載入完成後呼叫 `lucide.createIcons()`
2. 在 Vue 組件的 `onMounted` 中呼叫
3. 在動態更新 DOM 內容後重新呼叫
4. 使用防護檢查確保 lucide library 已載入
5. 使用 setTimeout 確保 DOM 更新完成

**常見使用場景**:
- 靜態頁面：在 `@section Scripts` 中初始化
- Vue 組件：在 `onMounted` 中初始化  
- 動態內容：在內容更新後重新初始化
- AJAX 載入：在成功回呼中初始化

**相關檔案**: 
- `Views/About/index.cshtml:81-91`
- `wwwroot/js/pages/about/about.js:52-55`

**關鍵概念**:
- Lucide icons 需要 JavaScript 初始化才能顯示
- `data-lucide` 屬性只是標記，需要 `createIcons()` 轉換
- DOM 更新後需要重新初始化 icons
- 防護性程式設計確保 library 載入