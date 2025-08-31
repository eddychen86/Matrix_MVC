# 問題 26: Razor 頁面中 Vue.js @error 事件語法編譯錯誤

**症狀**: 在 ASP.NET Core Razor 頁面中使用 Vue.js 的 `@error` 事件處理器時，出現編譯錯誤 "error CS0103: The name 'error' does not exist in the current context"。

**原因**: 
- ASP.NET Core Razor 引擎將 `@` 符號識別為 Razor 語法關鍵字
- 當在 HTML 屬性中使用 `@error` 時，Razor 編譯器嘗試將 `error` 解析為 C# 變數或方法
- Vue.js 事件處理語法 `@error` 與 Razor 語法衝突

**錯誤寫法**:
```html
<!-- 這會導致編譯錯誤 -->
<img v-if="item.image && item.image.filePath && !item.imageError" 
     :src="item.image.filePath" 
     @error="handleImageError(item, 'image')" />

<img v-if="item.authorAvatar && !item.avatarError" 
     :src="item.authorAvatar" 
     @error="handleImageError(item, 'avatar')" />
```

**正確寫法**:
```html
<!-- 使用 @@ 轉義語法 -->
<img v-if="item.image && item.image.filePath && !item.imageError" 
     :src="item.image.filePath" 
     @@error="handleImageError(item, 'image')" />

<img v-if="item.authorAvatar && !item.avatarError" 
     :src="item.authorAvatar" 
     @@error="handleImageError(item, 'avatar')" />
```

**解決方案**: 
1. **使用 Razor 轉義語法**:
   - 將 `@error` 改為 `@@error`
   - 第一個 `@` 告訴 Razor 引擎轉義處理
   - 第二個 `@` 作為字面字符輸出到 HTML

2. **適用於所有 Vue.js 事件**:
   - `@click` → `@@click`
   - `@input` → `@@input`
   - `@submit` → `@@submit`
   - `@error` → `@@error`

3. **編譯驗證**:
   - 使用 `dotnet build` 確認編譯無錯誤
   - 檢查瀏覽器中 HTML 輸出是否正確顯示為 `@error`

**技術細節**:
- Razor 引擎在伺服器端處理，Vue.js 在客戶端執行
- `@@` 轉義讓 Razor 引擎輸出單一 `@` 符號到 HTML
- 瀏覽器接收到的最終 HTML 中會正確顯示 `@error` 屬性
- Vue.js 能夠正常識別和處理事件綁定

**錯誤訊息範例**:
```
/Views/Home/Index.cshtml(25,35): error CS0103: The name 'error' does not exist in the current context
/Views/Home/Index.cshtml(42,43): error CS0103: The name 'error' does not exist in the current context
```

**相關檔案**: 
- `Views/Home/Index.cshtml:25, 42`
- 適用於所有包含 Vue.js 事件處理的 `.cshtml` 檔案