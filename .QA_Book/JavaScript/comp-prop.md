# 問題 18: Vue 組件屬性跨頁面可用性問題

**症狀**: 當 Vue 組件在不同頁面使用時，出現 "Property 'openModal'、'showPostModal'、'replyModal.article' was accessed during render but is not defined" 錯誤。

**原因**: 
1. `useCreatePost` 組件只在首頁條件載入，但 CreatePostPopup 在所有頁面都會渲染
2. main.js 使用 `LoadingPage` 函數按路徑條件載入組件，導致屬性只在特定頁面可用  
3. 跨頁面共用的組件未正確載入到全域作用域

**錯誤寫法**:
```javascript
// main.js - 條件載入但全域使用
const Home = LoadingPage(/^\\/(?:home(?:\\/|$))?$|^\\/$/i, useHome)  // 包含 useCreatePost
const Reply = LoadingPage(/^\\/reply(?:\\/|$)/i, useReply)  // ❌ 只在特定路徑載入

// home.js - 本地載入 createPost
const createPost = useCreatePost()
return { 
    ...createPost,  // openModal, showPostModal 只在首頁可用
}
```

**正確寫法**:
```javascript  
// main.js - 全域載入跨頁面組件
import { useCreatePost } from '/js/components/create-post.js'  // 新增導入

// 組件初始化 - 跨頁面組件全域載入
const Reply = (typeof useReply === 'function') ? useReply() : {}  // ✅ 總是載入
const CreatePost = (typeof useCreatePost === 'function') ? useCreatePost() : {}  // ✅ 新增全域載入

// 合併到全域作用域
return {
    ...Reply,
    ...CreatePost,  // openModal, showPostModal 現在全域可用
    ...Home,  // 移除 home.js 中的本地 createPost
}

// home.js - 移除本地載入避免重複
// import { useCreatePost } from '/js/components/create-post.js'  // 註解掉
// const createPost = useCreatePost()  // 註解掉
return {
    // ...createPost,  // 移除，現在全域可用
    toggleCreatePost, openCreatePost,
    // 其他 home 專屬屬性
}
```

**Vue 模板安全渲染**:
```html
<!-- CreatePostPopup 組件現在可在任何頁面安全使用 -->
<div v-if="showPostModal" class="modal">  <!-- ✅ showPostModal 全域可用 -->
    <!-- ... -->
</div>

<!-- ReplyPopup 組件現在可在任何頁面安全使用 --> 
<div v-if="replyModal.visible">
    <p>{{ replyModal.article.content }}</p>  <!-- ✅ 安全的初始結構 -->
</div>
```

**解決方案**:
1. **識別跨頁面組件**: 檢查哪些組件在多個頁面使用（如 CreatePostPopup、ReplyPopup）
2. **全域載入**: 將跨頁面組件移至 main.js 全域載入，使用 `typeof` 檢查防止錯誤
3. **移除重複載入**: 在特定頁面（如 home.js）中移除本地載入，避免屬性重複  
4. **安全初始化**: 確保組件屬性有安全的初始值，避免深層屬性存取錯誤
5. **導入聲明**: 在 main.js 頂部正確導入需要全域載入的組件

**偵錯技巧**:
- 使用 `Grep` 工具搜索屬性使用位置：`openModal`, `showPostModal`, `replyModal.article`
- 檢查組件在哪些頁面的 .cshtml 文件中被使用
- 確認 main.js 的 LoadingPage vs 全域載入策略
- 使用瀏覽器 Vue DevTools 檢查屬性是否正確合併到全域作用域

**關鍵洞察**:
- Vue 組件的可用性取決於其載入範圍（條件 vs 全域）
- 跨頁面共用的組件必須全域載入才能保證屬性一致性
- main.js 的組件合併策略 (`...Component`) 決定了屬性的最終可用性

**相關檔案**:
- `/wwwroot/js/main.js:7, 196, 319` (新增全域載入)
- `/wwwroot/js/pages/home/home.js:1, 127, 130` (移除本地載入)
- `/Views/Home/Index.cshtml:86` (CreatePostPopup)
- `/Views/Profile/Index.cshtml:118` (ReplyPopup)