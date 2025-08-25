# 問題 20: Tailwind CSS 響應式佈局衝突

**症狀**: 使用 `flex sm:block 2xl:flex` 時，在 sm 到 2xl 之間的斷點範圍內，元素不會換行且佈局異常

**原因**: `display: block` 與 flexbox 屬性（`items-center`, `justify-between`）產生邏輯衝突

**問題分析**:
- 預設（< 640px）：`display: flex` + flexbox 屬性 ✅ 正常
- sm（640px - 1535px）：`display: block` + flexbox 屬性 ❌ 衝突
- 2xl（≥ 1536px）：`display: flex` + flexbox 屬性 ✅ 正常

**錯誤寫法**:
```html
<div class="
    w-full
    flex sm:block 2xl:flex gap-2
    items-center justify-between
">
    <h5>標題</h5>
    <div>內容</div>
</div>
```

**正確寫法**:
```html
<!-- 方案 1: 使用 flex-direction 控制方向 -->
<div class="
    w-full
    flex flex-row sm:flex-col 2xl:flex-row gap-2
    items-center sm:items-start 2xl:items-center
    justify-between sm:justify-start 2xl:justify-between
">
    <h5>標題</h5>
    <div>內容</div>
</div>

<!-- 方案 2: 使用 Grid 佈局 -->
<div class="
    w-full grid grid-cols-2 sm:grid-cols-1 2xl:grid-cols-2 gap-2
    items-center sm:items-start 2xl:items-center
">
    <h5>標題</h5>
    <div class="sm:justify-self-start 2xl:justify-self-end">內容</div>
</div>

<!-- 方案 3: 混合佈局 -->
<div class="
    w-full
    flex sm:block 2xl:flex gap-2
">
    <h5 class="flex-shrink-0">標題</h5>
    <div class="sm:mt-2 2xl:mt-0 sm:w-full 2xl:w-auto ml-auto">內容</div>
</div>
```

**解決方案**: 
- **方案 1（推薦）**：使用 `flex-row`/`flex-col` 代替 `flex`/`block`，保持 flexbox 容器特性
- **方案 2**：使用 CSS Grid 進行響應式佈局控制
- **方案 3**：如果必須使用 `block`，則移除衝突的 flexbox 屬性，手動處理間距和對齊

**Tailwind 響應式斷點**:
- `sm`: ≥ 640px
- `md`: ≥ 768px  
- `lg`: ≥ 1024px
- `xl`: ≥ 1280px
- `2xl`: ≥ 1536px

**相關檔案**: 
- `Areas/Dashboard/Views/Config/index.cshtml:43-48`

**關鍵概念**:
- CSS 屬性的邏輯一致性
- Flexbox 屬性只能在 `display: flex` 容器中生效
- 響應式設計中不同斷點的屬性需要協調一致
- Tailwind CSS 斷點範圍的理解