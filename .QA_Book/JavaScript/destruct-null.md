# 問題 2: JavaScript 解構賦值 null 物件錯誤

**症狀**: `Uncaught (in promise) TypeError: Cannot destructure property 'articleId' of 'item' as it is null.`

**原因**: 函數參數解析邏輯錯誤，導致嘗試對 `null` 物件進行解構賦值操作

**問題分析**:
- PostList 組件呼叫：`stateFunc('report', item)` 
- profile.js 轉發：`postActions.stateFunc(type, aid)` (aid 是整個 item 物件)
- usePostActions.js 解析：當第二個參數是物件時，第三個參數 `item` 預設為 `null`
- report case 嘗試解構：`const { articleId, authorId } = item` (item 是 null)

**錯誤寫法**:
```javascript
case 'report': {
    const { articleId, authorId } = item  // item 是 null，因為參數解析邏輯問題
    return await openReport(articleId, authorId, row)
}
```

**正確寫法**:
```javascript
case 'report': {
    // 從 row (已處理過的 item) 或 articleIdOrPayload 中解構所需資料
    let reportArticleId = articleId
    let authorId = null
    
    // 如果 row 存在且包含必要資訊，優先使用 row
    if (row && row.articleId) {
        reportArticleId = row.articleId
        authorId = row.authorId || row.authorName // 嘗試獲取 authorId 或 authorName
    }
    // 如果 articleIdOrPayload 是物件且包含 authorId，則使用它
    else if (typeof articleIdOrPayload === 'object' && articleIdOrPayload.authorId) {
        reportArticleId = articleIdOrPayload.articleId
        authorId = articleIdOrPayload.authorId
    }
    
    if (!reportArticleId) {
        console.error('report action requires articleId')
        return false
    }
    if (!authorId) {
        console.error('report action requires authorId or authorName')
        return false
    }
    
    return await openReport(reportArticleId, authorId, row)
}
```

**解決方案**: 
- 理解函數參數解析邏輯，正確使用已處理的 `row` 變數
- 實作多層級資料來源檢查（`row` → `articleIdOrPayload`）
- 提供備用方案（`authorId` 或 `authorName`）
- 在解構前加入完善的防護檢查和錯誤處理

**相關檔案**: 
- `wwwroot/js/hooks/usePostActions.js:228-254`
- `Views/Shared/Components/PostList/Default.cshtml:83`
- `wwwroot/js/pages/profile/profile.js:760-762`

**關鍵概念**:
- JavaScript 解構賦值無法處理 `null`/`undefined`
- 函數參數解析與多重呼叫鏈的複雜性
- 防護性程式設計的重要性
- 多層級資料來源的備用方案設計