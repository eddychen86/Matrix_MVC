# 檔案變更詳細清單

## 📁 新增檔案

### SignalR 核心系統
```
Hubs/
├── MatrixHub.cs                     # SignalR Hub 主要功能
    ├── 用戶連接管理
    ├── 自動群組分配（Users, Admins）
    ├── 心跳檢測 (Ping/Pong)
    └── 手動群組加入/離開

Services/
├── SignalRService.cs                # SignalR 服務實現
│   ├── 功能開關廣播
│   ├── 系統公告發送
│   ├── 個人通知推送
│   ├── 互動更新廣播 (按讚/收藏)
│   ├── 追蹤更新廣播
│   ├── 管理員通知
│   └── 統計數據更新
└── Interfaces/
    └── ISignalRService.cs           # SignalR 服務介面定義
```

### 併發安全系統
```
Services/
└── ConcurrencySafeInteractionService.cs  # 併發安全互動服務
    ├── 樂觀併發控制
    ├── 重試機制 (指數延遲)
    ├── 交易安全 (TransactionScope)
    ├── 按讚功能 (TogglePraiseAsync)
    ├── 收藏功能 (ToggleCollectAsync)
    ├── 追蹤功能 (ToggleFollowAsync)
    └── 批量操作 (BatchProcessInteractionsAsync)
```

### API 控制器
```
Controllers/Api/
├── PraiseController.cs              # 按讚功能 API
│   ├── POST toggle/{articleId}      # 切換按讚狀態
│   ├── GET status/{articleId}       # 獲取按讚狀態
│   ├── POST batch                   # 批量按讚操作
│   └── GET my-praises               # 我的按讚列表
└── CollectsController.cs            # 收藏功能 API
    ├── POST toggle/{articleId}      # 切換收藏狀態
    ├── GET status/{articleId}       # 獲取收藏狀態
    ├── POST batch                   # 批量收藏操作
    ├── GET my-collections           # 我的收藏列表
    └── GET popular                  # 熱門收藏文章
```

### 前端 SignalR
```
wwwroot/js/signalr/
└── matrix-signalr.js                # 前端 SignalR 連接管理
    ├── 自動連接/重連
    ├── 事件處理註冊
    ├── 心跳機制
    ├── 群組管理
    ├── 功能開關處理
    ├── 通知顯示
    ├── 互動更新處理
    └── 錯誤處理
```

### 範例和文檔
```
Examples/
└── SignalRUsageExample.cs          # SignalR 使用範例
    ├── 功能開關範例
    ├── 社交互動範例
    ├── 通知發送範例
    ├── 管理員功能範例
    └── Controller 整合範例

wwwroot/static/signalR/
├── README.md                        # 主要設定說明
├── ConcurrencySafeService.md        # 併發安全服務詳細說明
├── ApiEndpoints.md                  # API 端點詳細說明
└── FileChanges.md                   # 本檔案
```

## 📝 修改檔案

### 1. Program.cs
```csharp
// 新增服務註冊
builder.Services.AddSignalR();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<ConcurrencySafeInteractionService>();

// 新增 Hub 路由
app.MapHub<Matrix.Hubs.MatrixHub>("/matrixHub");
```

### 2. Views/Shared/_Layout.cshtml
```html
<!-- 新增 SignalR 腳本引用 -->
<script src="~/lib/signalr/signalr.min.js"></script>
<script src="~/js/signalr/matrix-signalr.js" asp-append-version="true"></script>
```

### 3. Models/Article.cs
```csharp
// 新增併發控制欄位
[Timestamp]
public byte[] RowVersion { get; set; } = null!;
```
**用途**: 樂觀併發控制，防止同時更新衝突

### 4. Repository/ArticleRepository.cs
```csharp
// 新增原子操作方法
public async Task<bool> IncreasePraiseCountAtomicAsync(Guid articleId)
public async Task<bool> DecreasePraiseCountAtomicAsync(Guid articleId)
public async Task<bool> IncreaseCollectCountAtomicAsync(Guid articleId)
public async Task<bool> DecreaseCollectCountAtomicAsync(Guid articleId)

// 修改併發安全更新
private async Task<bool> UpdateArticleCountAsync(Guid articleId, Action<Article> updateAction)
```
**變更**: 
- 新增 SQL 層級原子操作
- 增加重試機制和併發衝突處理

### 5. Repository/Interfaces/IArticleRepository.cs
```csharp
// 新增原子操作介面
Task<bool> IncreasePraiseCountAtomicAsync(Guid articleId);
Task<bool> DecreasePraiseCountAtomicAsync(Guid articleId);
Task<bool> IncreaseCollectCountAtomicAsync(Guid articleId);
Task<bool> DecreaseCollectCountAtomicAsync(Guid articleId);
```

### 6. Controllers/Api/FollowsController.cs
```csharp
// 修改現有方法使用併發安全服務
[HttpPost("{targetId}")]
public async Task<IActionResult> FollowUser(Guid targetId)
{
    // 改用 ConcurrencySafeInteractionService.ToggleFollowAsync
}

[HttpDelete("{targetId}")]
public async Task<IActionResult> UnfollowUser(Guid targetId)
{
    // 改用 ConcurrencySafeInteractionService.ToggleFollowAsync
}

// 新增統一端點
[HttpPost("toggle/{targetId}")]
public async Task<IActionResult> ToggleFollow(Guid targetId)
```
**變更**:
- 原有端點改用併發安全服務
- 新增統一切換端點
- 增加完整錯誤處理
- 整合 SignalR 即時更新

## 🗄️ 資料庫變更

### 需要執行的遷移
```bash
dotnet ef migrations add AddArticleRowVersionForConcurrency
dotnet ef database update
```

### 新增欄位
```sql
-- Articles 表新增欄位
ALTER TABLE Articles ADD RowVersion rowversion NOT NULL;
```

## 🔧 設定變更

### appsettings.json (無變更)
現有設定足夠，無需額外配置。

### 依賴注入變更
```csharp
// Program.cs 中新增的服務
builder.Services.AddSignalR();                              // SignalR 核心服務
builder.Services.AddScoped<ISignalRService, SignalRService>();  // SignalR 業務服務
builder.Services.AddScoped<ConcurrencySafeInteractionService>();  // 併發安全服務
```

## 🌐 路由變更

### 新增 SignalR Hub 路由
```csharp
app.MapHub<Matrix.Hubs.MatrixHub>("/matrixHub");
```

### 新增 API 路由
```
POST /api/praise/toggle/{articleId}
GET  /api/praise/status/{articleId}
POST /api/praise/batch
GET  /api/praise/my-praises

POST /api/collects/toggle/{articleId}
GET  /api/collects/status/{articleId}
POST /api/collects/batch
GET  /api/collects/my-collections
GET  /api/collects/popular

POST /api/follows/toggle/{targetId}    # 新增
```

## 📊 影響分析

### 現有功能影響
| 功能 | 影響程度 | 說明 |
|------|----------|------|
| 按讚功能 | ✅ 增強 | 新增併發安全 + SignalR 即時更新 |
| 收藏功能 | ✅ 增強 | 新增併發安全 + SignalR 即時更新 |
| 追蹤功能 | ✅ 增強 | 新增併發安全 + SignalR 即時更新 |
| 用戶認證 | ⚪ 無影響 | 繼續使用現有 JWT 機制 |
| 現有 UI | ⚪ 無影響 | 可直接調用新 API |

### 效能影響
| 項目 | 變更 | 影響 |
|------|------|------|
| 資料庫 | +RowVersion 欄位 | 微小影響，增加 8 bytes |
| 併發處理 | +重試機制 | 衝突時略增延遲，整體更穩定 |
| 即時更新 | +SignalR 連接 | 增加 WebSocket 連接數 |
| API 回應 | 無變更 | 回應格式保持一致 |

### 安全性提升
- ✅ 防止併發操作導致的資料不一致
- ✅ 交易安全確保操作原子性
- ✅ 完整錯誤處理避免資訊洩漏
- ✅ JWT 認證整合無安全漏洞

## 🚀 部署建議

### 1. 資料庫遷移（必須）
```bash
# 在生產環境執行
dotnet ef database update --connection "ProductionConnectionString"
```

### 2. 服務重啟
重新啟動應用程式以載入新的 SignalR 服務。

### 3. 監控指標
- SignalR 連接數
- 併發衝突重試次數
- API 回應時間
- 錯誤率

### 4. 回滾計劃
如有問題可回滾到以下狀態：
- 資料庫：保留 RowVersion 欄位（向下相容）
- 程式碼：移除新增檔案，恢復修改檔案
- 設定：移除 SignalR 相關註冊

## ✅ 驗證清單

### 功能驗證
- [ ] SignalR 連接正常建立
- [ ] 按讚操作觸發即時更新
- [ ] 收藏操作觸發即時更新  
- [ ] 追蹤操作觸發即時更新
- [ ] 併發操作數據正確性
- [ ] 錯誤處理正常運作

### 效能驗證
- [ ] 多用戶同時操作無衝突
- [ ] SignalR 連接穩定
- [ ] API 回應時間正常
- [ ] 資料庫效能無異常

### 安全驗證
- [ ] JWT 認證正常運作
- [ ] 無授權漏洞
- [ ] 錯誤訊息不洩漏敏感資訊
- [ ] 併發控制機制有效