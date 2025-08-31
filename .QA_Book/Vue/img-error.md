# 問題 24: 圖片載入錯誤處理與動態顯示切換

**症狀**: hotlist 輪播中的圖片當 src 路徑無效時會顯示 alt 文字，需要自動偵測圖片載入錯誤並切換到 "no image" 佔位符顯示模式。

**原因**: 
- HTML `<img>` 標籤沒有直接的 "error" 屬性，需要使用 `onerror` 事件來偵測載入失敗
- Vue.js 需要響應式狀態來控制圖片顯示與佔位符之間的切換
- 原本只檢查圖片路徑是否存在，但無法處理路徑存在但實際載入失敗的情況

**錯誤寫法**:
```html
<!-- 只檢查路徑存在，無法處理載入失敗 -->
<img v-if="item.image && item.image.filePath" :src="item.image.filePath" />
<div v-else class="no-img">...</div>
```

**正確寫法**:
```html
<!-- 1. 模板部分 - 添加錯誤狀態檢查和 onerror 事件處理 -->
<img v-if="item.image && item.image.filePath && !item.imageError" 
     :src="item.image.filePath" 
     @error="handleImageError(item, 'image')" />
<div v-else class="no-img bg-accent size-full flex items-center justify-center">
    <i data-lucide="image" class="size-24 text-stone-600"></i>
</div>

<!-- 頭像處理 -->
<img v-if="item.authorAvatar && !item.avatarError" 
     :src="item.authorAvatar" 
     :alt="item.authorName" 
     class="size-full" 
     @error="handleImageError(item, 'avatar')" />
<div v-else class="no-img size-full flex items-center justify-center">
    <span>{{ (item.authorName || '').slice(0, 1).toUpperCase() }}</span>
</div>
```

```javascript
// 2. Vue.js 部分 - 初始化錯誤狀態和錯誤處理方法
const fetchHotList = async () => {
    try {
        const resp = await fetch('/api/post/hot?count=10', { credentials: 'same-origin' })
        if (!resp.ok) throw new Error('HTTP ' + resp.status)
        const data = await resp.json()
        // 為每個項目初始化圖片錯誤狀態
        hotlist.value = Array.isArray(data?.items) ? data.items.map(item => ({
            ...item,
            imageError: false,
            avatarError: false
        })) : []
        await nextTick()
        window.lucide?.createIcons?.()
        updateEdge()
    } catch (e) {
        console.error('Failed to load hot list:', e)
        hotlist.value = []
        updateEdge()
    }
}

// 處理圖片載入錯誤
const handleImageError = (item, type) => {
    if (!item) return
    
    // 根據類型設置對應的錯誤狀態
    if (type === 'image') {
        item.imageError = true
    } else if (type === 'avatar') {
        item.avatarError = true
    }
    
    console.log(`圖片載入失敗 - 文章ID: ${item.articleId}, 類型: ${type}`)
}

// 在 return 中導出方法
return {
    // ...other methods
    handleImageError
}
```

**解決方案**: 
1. **模板層面**: 
   - 在圖片顯示條件中添加 `&& !item.imageError` 和 `&& !item.avatarError`
   - 為 `<img>` 標籤添加 `@error="handleImageError(item, type)"` 事件處理器

2. **數據層面**:
   - 在 `fetchHotList()` 中為每個項目初始化 `imageError` 和 `avatarError` 狀態為 `false`
   - 實作 `handleImageError(item, type)` 方法來處理圖片載入失敗

3. **響應式更新**:
   - 當圖片載入失敗時，`handleImageError` 會將對應的錯誤狀態設為 `true`
   - Vue.js 響應式系統自動重新評估條件表達式，切換到 "no image" 模式

**技術細節**:
- 使用 HTML5 `onerror` 事件 (不是 "error" 屬性)
- Vue.js 響應式更新通過直接修改對象屬性觸發
- 支持兩種圖片類型：主圖片 (`image`) 和頭像 (`avatar`)
- 錯誤狀態持久化，避免重複載入失敗的圖片

**相關檔案**: 
- `Views/Home/Index.cshtml:23-25, 38-42`
- `wwwroot/js/pages/home/home.js:17-22, 138-150, 182`