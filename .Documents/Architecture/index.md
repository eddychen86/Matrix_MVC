# 架構設計技術文件索引

**分類**: Architecture & System Design  
**技術領域**: 系統架構、即時通訊、擴展性設計、效能優化  

## 📋 文件列表

### 文件 1: SignalR 即時通訊架構
**檔案**: [`signalr-realtime.md`](./signalr-realtime.md)  
**描述**: SignalR Core 即時通訊系統完整解析，包含 Hub 設計、連線管理、群組功能、前端整合等  
**關鍵字**: SignalR, Real-time Communication, WebSocket, Hub, Connection Management, Group Chat, Push Notification  
**相關檔案**: Hubs/MatrixHub.cs, wwwroot/js/signalr/, Services/SignalRService.cs  
**複雜度**: 中級到高級  

**內容概要**:
- SignalR Hub 核心架構
- 連線生命週期管理
- 即時訊息傳遞機制
- 群組和私人聊天功能
- 前端 JavaScript 客戶端整合
- 效能優化與擴展策略
- 故障恢復與重連機制

---

## 🎯 學習路線

### 入門階段 (1-2 週)
1. **SignalR 概念**: 理解即時通訊的基本原理和應用場景
2. **Hub 基礎**: 學習 SignalR Hub 的基本用法
3. **連線管理**: 掌握用戶連線和斷線處理

### 進階階段 (2-4 週)  
1. **群組功能**: 實作聊天室和群組管理
2. **訊息持久化**: 整合資料庫存放聊天記錄  
3. **前端整合**: 建立完整的前端即時通訊介面

### 專家階段 (2-3 週)
1. **效能優化**: 學習大規模連線的效能調優
2. **擴展架構**: 實作多伺服器擴展和負載平衡
3. **監控診斷**: 建立即時通訊的監控和故障排除機制

---

## 🔗 技術關聯

### 通訊協議
- **WebSocket**: 主要的雙向通訊協議
- **Server-Sent Events**: 降級方案之一
- **Long Polling**: 最後的降級方案
- **HTTP/2**: 現代 Web 應用程式的基礎協議

### 後端整合
- **ASP.NET Core**: 基礎 Web 框架
- **Entity Framework**: 訊息持久化
- **Memory Cache**: 連線狀態快取
- **Redis**: 分散式連線狀態管理

### 前端整合
- **SignalR JavaScript Client**: 官方前端客戶端
- **Vue.js**: 前端框架整合
- **Browser WebSocket API**: 底層通訊介面
- **Progressive Web App**: 行動裝置推送通知

---

## 🏗️ 系統架構

### 單機架構
```
Browser Client
    ↓ WebSocket
SignalR Hub (MatrixHub)  
    ↓
Connection Manager (記憶體)
    ↓  
Business Services
    ↓
Database (Message Storage)
```

### 分散式架構
```
Multiple Browser Clients
    ↓ WebSocket  
Load Balancer
    ↓
Multiple SignalR Servers
    ↓
Redis Backplane (訊息分發)
    ↓
Shared Database Cluster
```

---

## 🔄 即時通訊流程

### 私人訊息流程
```csharp
// 1. 用戶 A 發送訊息
await hub.SendPrivateMessage("user-b-id", "Hello");

// 2. Hub 處理訊息
public async Task SendPrivateMessage(string targetUserId, string message)
{
    // 儲存到資料庫
    var messageId = await _messageService.CreateAsync(senderId, targetUserId, message);
    
    // 發送給目標用戶
    await Clients.Group($"user_{targetUserId}")
                 .SendAsync("ReceivePrivateMessage", messageData);
}

// 3. 前端接收訊息
connection.on('ReceivePrivateMessage', (data) => {
    displayMessage(data);
    updateUnreadCount();
});
```

### 群組聊天流程
```csharp
// 1. 用戶加入聊天室
await hub.JoinChatRoom("general");

// 2. Hub 處理加入請求
public async Task JoinChatRoom(string roomName)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    
    await Clients.Group(roomName)
                 .SendAsync("UserJoinedRoom", userInfo);
}

// 3. 廣播群組訊息
public async Task SendRoomMessage(string roomName, string message)
{
    await Clients.Group(roomName)
                 .SendAsync("ReceiveRoomMessage", messageData);
}
```

---

## 💡 設計模式與最佳實務

### 連線狀態管理
```csharp
// 連線管理器模式
public class ConnectionManager
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    
    public void AddConnection(string userId, string connectionId)
    {
        _userConnections.AddOrUpdate(userId, 
            new HashSet<string> { connectionId },
            (key, existing) => {
                existing.Add(connectionId);
                return existing;
            });
    }
    
    public bool IsUserOnline(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }
}
```

### 訊息分發策略
- **單播**: 一對一私人訊息
- **群組播送**: 聊天室內所有成員
- **廣播**: 全站公告和系統通知
- **有條件廣播**: 基於用戶屬性的選擇性廣播

### 錯誤處理與重試
```javascript
// 前端重連機制
class SignalRManager {
    constructor() {
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }
    
    async startConnection() {
        try {
            await this.connection.start();
            this.reconnectAttempts = 0;
        } catch (error) {
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                setTimeout(() => this.startConnection(), 5000);
            }
        }
    }
}
```

---

## ⚡ 效能優化策略

### 伺服器端優化
- **連線池管理**: 合理設定最大連線數
- **訊息批次處理**: 減少 I/O 操作次數
- **非同步處理**: 避免阻塞主執行緒
- **記憶體快取**: 快取常用的連線資訊

### 客戶端優化
- **訊息去重**: 避免重複顯示相同訊息
- **虛擬滾動**: 大量歷史訊息的效能優化
- **離線快取**: 本地儲存未讀訊息
- **智能重連**: 網路恢復時自動重連

### 網路優化
```csharp
// MessagePack 協議優化
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false; // 生產環境關閉詳細錯誤
    options.MaximumReceiveMessageSize = 1024 * 1024; // 限制訊息大小
})
.AddMessagePackProtocol(); // 使用 MessagePack 提升效能
```

---

## 🔍 監控與診斷

### 效能指標監控
- **連線數量**: 即時監控線上用戶數
- **訊息吞吐量**: 每秒處理的訊息數量
- **延遲統計**: 訊息傳遞的平均延遲
- **錯誤率**: 連線失敗和訊息遺失率

### 診斷工具
```csharp
// 自定義診斷中介軟體
public class SignalRDiagnosticsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var stopwatch = Stopwatch.StartNew();
            
            await _next(context);
            
            stopwatch.Stop();
            _logger.LogInformation("WebSocket request took {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### 日誌記錄
- **連線事件**: 記錄用戶上線/離線
- **訊息追蹤**: 重要訊息的傳遞日誌
- **錯誤日誌**: 連線錯誤和異常情況
- **效能日誌**: 慢查詢和效能瓶頸

---

## 🌐 擴展性考量

### 水平擴展
```csharp
// Redis Backplane 設定
builder.Services.AddSignalR()
    .AddStackExchangeRedis(connectionString, options =>
    {
        options.Configuration.ChannelPrefix = "Matrix";
    });
```

### 垂直擴展  
- **硬體升級**: CPU、記憶體、網路頻寬
- **程式碼優化**: 減少記憶體使用和 CPU 消耗
- **資料庫優化**: 查詢優化和索引建立

### 混合架構
- **分層處理**: 不同類型的訊息使用不同的伺服器
- **地理分布**: 多地區部署減少延遲
- **CDN 整合**: 靜態資源通過 CDN 分發

---

## 📚 推薦學習資源

### 官方文件
- [SignalR 官方文檔](https://docs.microsoft.com/aspnet/core/signalr/)
- [JavaScript 客戶端指南](https://docs.microsoft.com/aspnet/core/signalr/javascript-client)
- [擴展性指南](https://docs.microsoft.com/aspnet/core/signalr/scale)

### 進階主題
- [MessagePack 協議](https://msgpack.org/)
- [Redis Backplane](https://docs.microsoft.com/aspnet/core/signalr/redis-backplane)
- [Azure SignalR Service](https://docs.microsoft.com/azure/azure-signalr/)

### 實作範例
- Matrix 專案 SignalR 實作
- 即時聊天室完整範例
- 大規模即時通訊架構案例

---

**最後更新**: 2025-08-29  
**文件數量**: 1  
**總學習時間**: 5-9 週 (依個人基礎而定)