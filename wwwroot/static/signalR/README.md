# Matrix SignalR 即時更新系統設定說明

## 📋 概覽

本文檔說明 Matrix 專案的 SignalR 即時更新系統，包括按讚、收藏、追蹤功能的併發安全實作與即時通知。

## 🆕 新增檔案清單

### SignalR 核心系統
- `Hubs/MatrixHub.cs` - 通用 SignalR Hub
- `Services/SignalRService.cs` - SignalR 服務實現
- `Services/Interfaces/ISignalRService.cs` - SignalR 服務介面
- `wwwroot/js/signalr/matrix-signalr.js` - 前端連接管理

### 併發安全服務
- `Services/ConcurrencySafeInteractionService.cs` - 併發安全互動服務

### API 控制器
- `Controllers/Api/PraiseController.cs` - 按讚 API
- `Controllers/Api/CollectsController.cs` - 收藏 API

### 範例與文檔
- `Examples/SignalRUsageExample.cs` - 使用範例

## ✏️ 修改檔案清單

### 配置檔案
- `Program.cs` - 新增 SignalR 服務註冊
- `Views/Shared/_Layout.cshtml` - 新增 SignalR 腳本引用

### 資料模型
- `Models/Article.cs` - 新增 RowVersion 併發控制欄位

### Repository 層
- `Repository/ArticleRepository.cs` - 新增原子操作方法
- `Repository/Interfaces/IArticleRepository.cs` - 新增原子操作介面

### 現有 API 增強
- `Controllers/Api/FollowsController.cs` - 整合併發安全服務

## 🔧 設定步驟

### 1. 資料庫遷移（必要）
```bash
dotnet ef migrations add AddArticleRowVersionForConcurrency
dotnet ef database update
```

### 2. 服務註冊（已完成）
Program.cs 中已新增：
```csharp
// SignalR 服務
builder.Services.AddSignalR();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<ConcurrencySafeInteractionService>();

// Hub 路由
app.MapHub<Matrix.Hubs.MatrixHub>("/matrixHub");
```

### 3. 前端整合（已完成）
_Layout.cshtml 中已新增：
```html
<script src="~/lib/signalr/signalr.min.js"></script>
<script src="~/js/signalr/matrix-signalr.js"></script>
```

## 🎯 API 端點

### 按讚功能
- `POST /api/praise/toggle/{articleId}` - 切換按讚狀態
- `GET /api/praise/status/{articleId}` - 獲取按讚狀態
- `POST /api/praise/batch` - 批量按讚操作
- `GET /api/praise/my-praises` - 我的按讚列表

### 收藏功能
- `POST /api/collects/toggle/{articleId}` - 切換收藏狀態
- `GET /api/collects/status/{articleId}` - 獲取收藏狀態
- `POST /api/collects/batch` - 批量收藏操作
- `GET /api/collects/my-collections` - 我的收藏列表
- `GET /api/collects/popular` - 熱門收藏文章

### 追蹤功能
- `POST /api/follows/toggle/{targetId}` - 切換追蹤狀態（新增）
- 原有端點保持不變

## ⚡ SignalR 事件

### 功能開關變更
```javascript
// 監聽功能開關變更
window.matrixSignalR.on('featureToggle', (data) => {
    console.log('功能開關變更:', data.featureName, data.isEnabled);
});
```

### 互動更新（按讚/收藏）
```javascript
// 監聽互動更新
window.matrixSignalR.on('interactionUpdate', (update) => {
    console.log('互動更新:', update.action, update.newCount);
    // 自動更新 UI 中的計數
});
```

### 追蹤更新
```javascript
// 監聽追蹤更新
window.matrixSignalR.on('followUpdate', (update) => {
    console.log('追蹤更新:', update.isFollowing);
    // 自動更新追蹤按鈕狀態
});
```

### 新通知
```javascript
// 監聽新通知
window.matrixSignalR.on('newNotification', (notification) => {
    console.log('新通知:', notification.message);
    // 顯示通知 Toast
});
```

## 🛡️ 併發控制機制

### 三層防護
1. **樂觀併發控制** - Article.RowVersion 欄位
2. **重試機制** - 指數延遲自動重試
3. **原子操作** - SQL 層級安全更新

### 使用範例
```csharp
// 併發安全的按讚操作
var result = await _interactionService.TogglePraiseAsync(userId, articleId);
if (result.Success) {
    // 自動觸發 SignalR 即時更新
    Console.WriteLine($"按讚狀態：{result.IsLiked}，新計數：{result.NewCount}");
}
```

## 🧪 測試方式

### 1. 檢查 SignalR 連接
開啟瀏覽器開發者工具 > Console，應該看到：
```
✅ MatrixSignalR 連接成功
```

### 2. 測試併發操作
開啟多個瀏覽器分頁，同時點擊按讚按鈕，觀察計數是否正確。

### 3. 測試即時更新
在一個分頁執行操作，其他分頁應該即時看到更新。

## 🔍 故障排除

### SignalR 連接失敗
- 檢查 `/matrixHub` 端點是否正確註冊
- 確認 SignalR 客戶端腳本正確載入

### 併發操作失敗
- 檢查資料庫是否正確執行遷移
- 確認 Article.RowVersion 欄位存在

### API 401 錯誤
- 確認 JWT 認證正常運作
- 檢查 PersonId 與 UserId 對應關係

## 📝 注意事項

1. **資料庫遷移必須執行**，否則會因為缺少 RowVersion 欄位而報錯
2. **現有 UI 無需修改**，可直接調用新的 API 端點
3. **SignalR 自動初始化**，頁面載入時會自動建立連接
4. **併發控制是可選的**，主要依賴原子操作確保資料正確性

## 📚 相關文檔

- [SignalR 使用範例](../../../Examples/SignalRUsageExample.cs)
- [併發安全服務文檔](./ConcurrencySafeService.md)
- [API 端點詳細說明](./ApiEndpoints.md)