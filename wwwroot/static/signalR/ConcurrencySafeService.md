# 併發安全服務詳細說明

## 🎯 目的

`ConcurrencySafeInteractionService` 解決多用戶同時操作導致的資料競爭問題，確保按讚、收藏、追蹤功能的資料正確性。

## ⚠️ 問題場景

### 資料競爭範例
```
情境：兩個用戶同時按讚同一篇文章

時間點 1: 用戶A 讀取 PraiseCount = 100
時間點 2: 用戶B 讀取 PraiseCount = 100
時間點 3: 用戶A 計算 100 + 1 = 101，保存
時間點 4: 用戶B 計算 100 + 1 = 101，保存

結果：應該是 102，實際只有 101 ❌
```

## 🛡️ 解決方案

### 1. 樂觀併發控制（Optimistic Concurrency）

#### Article 模型新增欄位
```csharp
[Timestamp]
public byte[] RowVersion { get; set; } = null!;
```

#### 工作原理
```csharp
// EF Core 自動檢測併發衝突
try {
    await _context.SaveChangesAsync();
} catch (DbUpdateConcurrencyException) {
    // 自動重試機制
    await Task.Delay(100 * attempt);
    // 重新讀取最新資料並重試
}
```

### 2. 原子操作（Atomic Operations）

#### SQL 層級的安全更新
```csharp
// 直接在資料庫層面執行，無法被中斷
await _context.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE Articles SET PraiseCount = PraiseCount + 1 WHERE ArticleId = {articleId}"
);
```

### 3. 交易範圍（Transaction Scope）

#### 確保操作原子性
```csharp
using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

// 1. 新增/刪除 PraiseCollect 記錄
// 2. 更新 Article 計數
// 3. 全部成功才提交，否則全部回滾

transaction.Complete();
```

## 🔄 重試機制

### 指數延遲重試
```csharp
const int maxRetries = 3;

for (int attempt = 0; attempt < maxRetries; attempt++) {
    try {
        // 執行操作
        return success;
    } catch (DbUpdateConcurrencyException) {
        if (attempt == maxRetries - 1) throw;
        
        // 指數延遲：100ms, 200ms, 300ms
        await Task.Delay(100 * (attempt + 1));
    }
}
```

## 🚀 API 使用方式

### 按讚功能
```csharp
var result = await _interactionService.TogglePraiseAsync(userId, articleId);

// 結果包含：
// - Success: 操作是否成功
// - IsLiked: 新的按讚狀態
// - NewCount: 更新後的總計數
```

### 收藏功能
```csharp
var result = await _interactionService.ToggleCollectAsync(userId, articleId);

// 結果包含：
// - Success: 操作是否成功  
// - IsCollected: 新的收藏狀態
// - NewCount: 更新後的總計數
```

### 追蹤功能
```csharp
var result = await _interactionService.ToggleFollowAsync(followerId, followedId);

// 結果包含：
// - Success: 操作是否成功
// - IsFollowing: 新的追蹤狀態
```

## ⚡ SignalR 即時更新

### 自動廣播
每次成功操作後，系統會自動發送 SignalR 更新：

```csharp
// 按讚/收藏更新
await _signalRService.BroadcastInteractionUpdateAsync(
    articleId, 
    "praise", // 或 "collect"
    newCount, 
    authorId  // 通知文章作者
);

// 追蹤更新
await _signalRService.BroadcastFollowUpdateAsync(
    followerId, 
    followedId, 
    isFollowing
);
```

### 前端自動更新
```javascript
// 前端會自動接收並更新 UI
window.matrixSignalR.on('interactionUpdate', (update) => {
    // 更新頁面上的計數顯示
    document.querySelector(`[data-article-${update.action}-count="${update.articleId}"]`)
        .textContent = update.newCount;
});
```

## 📊 效能優化

### 批量操作
```csharp
// 同時處理多個操作，減少併發衝突
var interactions = [
    (userId, articleId1, "praise"),
    (userId, articleId2, "collect")
];

var results = await _interactionService.BatchProcessInteractionsAsync(interactions);
```

### 按文章分組
```csharp
// 自動按 ArticleId 分組，避免不同文章間的不必要等待
var groupedByArticle = interactions.GroupBy(i => i.ArticleId);
```

## 🔍 錯誤處理

### 常見錯誤類型
```csharp
try {
    var result = await _interactionService.TogglePraiseAsync(userId, articleId);
} catch (InvalidOperationException ex) {
    // 業務邏輯錯誤：如嘗試追蹤自己
    return BadRequest(ex.Message);
} catch (DbUpdateConcurrencyException ex) {
    // 併發衝突（通常已被重試機制處理）
    return BadRequest("操作失敗，請稍後重試");
}
```

### 日誌記錄
```csharp
_logger.LogInformation("用戶 {UserId} {Action} 文章 {ArticleId}", 
    userId, isLiked ? "按讚" : "取消按讚", articleId);

_logger.LogWarning("按讚操作併發衝突，嘗試 {Attempt}/{MaxRetries}：{Error}", 
    attempt + 1, maxRetries, ex.Message);
```

## 🧪 測試建議

### 併發測試
1. 開啟多個瀏覽器分頁
2. 同時快速點擊同一個按讚按鈕
3. 觀察最終計數是否正確

### 效能測試
```javascript
// 批量測試
const promises = [];
for (let i = 0; i < 10; i++) {
    promises.push(
        fetch(`/api/praise/toggle/${articleId}`, { method: 'POST' })
    );
}
await Promise.all(promises);
```

## 📈 監控指標

### 關鍵指標
- 併發衝突重試次數
- 操作成功率
- 平均響應時間
- SignalR 連接數

### 日誌查詢
```
# 查看併發衝突
grep "併發衝突" logs/

# 查看重試情況  
grep "嘗試.*次" logs/

# 查看操作統計
grep "按讚\|收藏\|追蹤" logs/ | wc -l
```

## 🔧 設定選項

### 重試設定
```csharp
const int maxRetries = 3;           // 最大重試次數
const int baseDelay = 100;          // 基礎延遲（毫秒）
// 實際延遲：100ms, 200ms, 300ms
```

### 交易設定
```csharp
using var transaction = new TransactionScope(
    TransactionScopeOption.Required,
    new TransactionOptions { 
        IsolationLevel = IsolationLevel.ReadCommitted,
        Timeout = TimeSpan.FromSeconds(30)
    },
    TransactionScopeAsyncFlowOption.Enabled
);
```