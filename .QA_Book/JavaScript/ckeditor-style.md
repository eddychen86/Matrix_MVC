# 問題 3: CKEditor 自訂樣式被覆蓋且 overflow 無效

**症狀**: CKEditor 中設定的 class 在點擊編輯區域時會變回預設樣式，且設定的 overflow 屬性無效

**原因**: 
1. CKEditor 有自己的 CSS 權重系統，會覆蓋外部樣式
2. CKEditor 的編輯區域實際上是在獨立的 DOM 結構中（.ck-editor__editable）
3. 單純設定容器的 overflow 無法控制內部編輯區域的滾動

**錯誤寫法**:
```css
.ck_cts {
    min-height: 150px;
    max-height: 500px;
    overflow-y: auto;
}
```

**正確寫法**:
```css
/* CKEditor 容器樣式 - 使用更高權重 */
.ck_cts.ck-editor {
    min-height: 150px !important;
    max-height: 500px !important;
}

/* CKEditor 編輯區域樣式 */
.ck_cts .ck-editor__editable {
    min-height: 150px !important;
    max-height: 500px !important;
    overflow-y: auto !important;
}

/* 確保 CKEditor 主容器有正確的滾動 */
.ck_cts .ck-editor__main {
    max-height: 500px !important;
    overflow: hidden !important;
}
```

**解決方案**: 
1. **CSS 端**: 使用多層選擇器覆蓋 CKEditor 預設樣式
2. **JavaScript 端**: 移除 onEditorReady 中衝突的 Tailwind 高度 class
3. 使用 `!important` 強制覆蓋 CKEditor 預設樣式
4. 確保 CSS 和 JavaScript 設定不衝突

**正確的完整解決步驟**:

**步驟1 - 修改 CSS** (Views/Shared/Components/CreatePostPopup/Default.cshtml):
```css
/* CKEditor 外層容器 */
.ck_cts {
    min-height: 150px !important;
    max-height: 500px !important;
    overflow: hidden !important;
}

/* CKEditor 主編輯器容器 - 最高權重 */
.ck_cts .ck.ck-editor {
    min-height: 150px !important;
    max-height: 500px !important;
    overflow: hidden !important;
}

/* CKEditor 可編輯區域 - 關鍵樣式 */
.ck_cts .ck.ck-editor .ck-editor__editable.ck-rounded-corners {
    min-height: 130px !important;
    max-height: 480px !important;
    overflow-y: auto !important;
    overflow-x: hidden !important;
}
```

**步驟2 - 修改 JavaScript** (wwwroot/js/components/create-post.js):
移除 onEditorReady 中的高度相關 class：
```javascript
// 原本的衝突代碼
editor.ui.view.editable.element.classList.add(
    'min-h-[150px]',  // ← 移除這行
    'max-h-[467px]',  // ← 移除這行
    'rounded-[10px]',
    'bg-transparent'
);

// 修正後
editor.ui.view.editable.element.classList.add(
    'rounded-[10px]',
    'bg-transparent'
);
```

**相關檔案**: 
- Views/Shared/Components/CreatePostPopup/Default.cshtml:3-38
- wwwroot/js/components/create-post.js:64-67