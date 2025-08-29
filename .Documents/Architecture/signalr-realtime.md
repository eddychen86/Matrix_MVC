# SignalR 即時通訊技術文件

**技術分類**: 即時通訊架構  
**複雜度**: 中級到高級  
**適用情境**: 即時聊天、通知推送、協作功能、即時資料更新  

## 技術概述

Matrix 專案使用 SignalR 實現即時雙向通訊功能，支援即時聊天、通知推送、線上狀態管理等功能，提供良好的用戶體驗。

## 基礎技術

### 1. SignalR 架構組件
```
Hubs/
├── MatrixHub.cs                  # 主要 Hub 類別
wwwroot/js/
├── signalr/
│   └── matrix-signalr.js         # 前端 SignalR 管理
Services/
├── SignalRService.cs             # SignalR 業務邏輯服務
└── NotificationService.cs        # 通知服務
```

### 2. Hub 設定 (Program.cs:129)
```csharp
// SignalR 服務註冊
builder.Services.AddSignalR();

// Hub 路由設定
app.MapHub<MatrixHub>("/matrixHub");
```

### 3. MatrixHub 核心實作 (Hubs/MatrixHub.cs)
```csharp
public class MatrixHub : Hub
{
    private readonly ILogger<MatrixHub> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private static readonly ConcurrentDictionary<string, UserConnection> _connections = new();

    public MatrixHub(
        ILogger<MatrixHub> logger, 
        INotificationService notificationService,
        IUserService userService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userService = userService;
    }

    // 用戶連線處理
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
        {
            var connection = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                UserName = userName,
                ConnectedAt = DateTime.UtcNow
            };

            _connections.TryAdd(Context.ConnectionId, connection);

            // 加入個人群組（用於私人通知）
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // 通知其他用戶此用戶上線
            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserOnline", new
            {
                UserId = userId,
                UserName = userName,
                ConnectionTime = DateTime.UtcNow
            });

            _logger.LogInformation("User {UserName} ({UserId}) connected with ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    // 用戶斷線處理
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var connection))
        {
            // 移除用戶群組
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{connection.UserId}");

            // 通知其他用戶此用戶離線
            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserOffline", new
            {
                UserId = connection.UserId,
                UserName = connection.UserName,
                DisconnectionTime = DateTime.UtcNow,
                Duration = DateTime.UtcNow - connection.ConnectedAt
            });

            _logger.LogInformation("User {UserName} ({UserId}) disconnected. Duration: {Duration}", 
                connection.UserName, connection.UserId, DateTime.UtcNow - connection.ConnectedAt);
        }

        if (exception != null)
        {
            _logger.LogError(exception, "User disconnected with error");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // 發送私人訊息
    [Authorize]
    public async Task SendPrivateMessage(string targetUserId, string message)
    {
        try
        {
            var senderId = GetUserId();
            var senderName = GetUserName();

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(targetUserId))
            {
                await Clients.Caller.SendAsync("Error", "無效的用戶 ID");
                return;
            }

            // 儲存訊息到資料庫
            var messageId = await _notificationService.CreatePrivateMessageAsync(senderId, targetUserId, message);

            var messageData = new
            {
                MessageId = messageId,
                SenderId = senderId,
                SenderName = senderName,
                TargetUserId = targetUserId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Type = "PrivateMessage"
            };

            // 發送給目標用戶
            await Clients.Group($"user_{targetUserId}").SendAsync("ReceivePrivateMessage", messageData);

            // 回傳確認給發送者
            await Clients.Caller.SendAsync("MessageSent", messageData);

            _logger.LogInformation("Private message sent from {SenderId} to {TargetUserId}", senderId, targetUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending private message");
            await Clients.Caller.SendAsync("Error", "發送訊息失敗");
        }
    }

    // 加入聊天室
    [Authorize]
    public async Task JoinChatRoom(string roomName)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            await Clients.Group(roomName).SendAsync("UserJoinedRoom", new
            {
                UserId = userId,
                UserName = userName,
                RoomName = roomName,
                Timestamp = DateTime.UtcNow
            });

            await Clients.Caller.SendAsync("JoinedRoom", new
            {
                RoomName = roomName,
                Message = $"已加入聊天室 {roomName}"
            });

            _logger.LogInformation("User {UserName} joined room {RoomName}", userName, roomName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining chat room {RoomName}", roomName);
            await Clients.Caller.SendAsync("Error", "加入聊天室失敗");
        }
    }

    // 離開聊天室
    [Authorize]
    public async Task LeaveChatRoom(string roomName)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

            await Clients.Group(roomName).SendAsync("UserLeftRoom", new
            {
                UserId = userId,
                UserName = userName,
                RoomName = roomName,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("User {UserName} left room {RoomName}", userName, roomName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving chat room {RoomName}", roomName);
        }
    }

    // 發送聊天室訊息
    [Authorize]
    public async Task SendRoomMessage(string roomName, string message)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();

            var messageData = new
            {
                SenderId = userId,
                SenderName = userName,
                RoomName = roomName,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Type = "RoomMessage"
            };

            await Clients.Group(roomName).SendAsync("ReceiveRoomMessage", messageData);

            _logger.LogInformation("Room message sent by {UserName} in room {RoomName}", userName, roomName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending room message");
            await Clients.Caller.SendAsync("Error", "發送訊息失敗");
        }
    }

    // 發送通知
    public async Task SendNotification(string targetUserId, string title, string message, string type = "Info")
    {
        try
        {
            var notificationData = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveNotification", notificationData);

            _logger.LogInformation("Notification sent to user {TargetUserId}: {Title}", targetUserId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
        }
    }

    // 廣播系統公告
    public async Task BroadcastAnnouncement(string title, string message, string type = "Announcement")
    {
        try
        {
            var announcementData = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveAnnouncement", announcementData);

            _logger.LogInformation("System announcement broadcasted: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting announcement");
        }
    }

    // 取得線上用戶列表
    [Authorize]
    public async Task GetOnlineUsers()
    {
        try
        {
            var onlineUsers = _connections.Values
                .Select(c => new
                {
                    UserId = c.UserId,
                    UserName = c.UserName,
                    ConnectedAt = c.ConnectedAt
                })
                .ToList();

            await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users");
            await Clients.Caller.SendAsync("Error", "取得線上用戶失敗");
        }
    }

    // 輔助方法
    private string GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "";
    }

    // 用戶連線資訊
    private class UserConnection
    {
        public string ConnectionId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTime ConnectedAt { get; set; }
    }
}
```

## 前端整合

### 1. SignalR JavaScript 客戶端 (matrix-signalr.js)
```javascript
class MatrixSignalR {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 5000; // 5 秒
        this.eventHandlers = new Map();
        
        this.initializeConnection();
    }

    // 初始化連線
    async initializeConnection() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/matrixHub', {
                    accessTokenFactory: () => {
                        // 從 Cookie 取得 JWT Token
                        const token = this.getTokenFromCookie();
                        return token;
                    }
                })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.previousRetryCount === 0) {
                            return 0;
                        }
                        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            this.setupEventHandlers();
            await this.startConnection();

        } catch (error) {
            console.error('SignalR 初始化失敗:', error);
        }
    }

    // 設定事件處理器
    setupEventHandlers() {
        // 連線狀態事件
        this.connection.onclose((error) => {
            this.isConnected = false;
            console.log('SignalR 連線已關閉', error);
            this.emit('disconnected', { error });
        });

        this.connection.onreconnecting((error) => {
            console.log('SignalR 重新連線中...', error);
            this.emit('reconnecting', { error });
        });

        this.connection.onreconnected((connectionId) => {
            this.isConnected = true;
            this.reconnectAttempts = 0;
            console.log('SignalR 重新連線成功:', connectionId);
            this.emit('reconnected', { connectionId });
        });

        // 訊息接收事件
        this.connection.on('ReceivePrivateMessage', (data) => {
            console.log('收到私人訊息:', data);
            this.emit('privateMessage', data);
        });

        this.connection.on('ReceiveRoomMessage', (data) => {
            console.log('收到聊天室訊息:', data);
            this.emit('roomMessage', data);
        });

        this.connection.on('ReceiveNotification', (data) => {
            console.log('收到通知:', data);
            this.emit('notification', data);
        });

        this.connection.on('ReceiveAnnouncement', (data) => {
            console.log('收到系統公告:', data);
            this.emit('announcement', data);
        });

        // 用戶狀態事件
        this.connection.on('UserOnline', (data) => {
            console.log('用戶上線:', data);
            this.emit('userOnline', data);
        });

        this.connection.on('UserOffline', (data) => {
            console.log('用戶離線:', data);
            this.emit('userOffline', data);
        });

        this.connection.on('OnlineUsers', (users) => {
            console.log('線上用戶:', users);
            this.emit('onlineUsers', users);
        });

        // 聊天室事件
        this.connection.on('UserJoinedRoom', (data) => {
            console.log('用戶加入聊天室:', data);
            this.emit('userJoinedRoom', data);
        });

        this.connection.on('UserLeftRoom', (data) => {
            console.log('用戶離開聊天室:', data);
            this.emit('userLeftRoom', data);
        });

        // 錯誤處理
        this.connection.on('Error', (error) => {
            console.error('SignalR 錯誤:', error);
            this.emit('error', { error });
        });
    }

    // 啟動連線
    async startConnection() {
        try {
            await this.connection.start();
            this.isConnected = true;
            console.log('SignalR 連線成功');
            this.emit('connected');
            
            // 取得線上用戶列表
            await this.getOnlineUsers();
            
        } catch (error) {
            console.error('SignalR 連線失敗:', error);
            this.isConnected = false;
            
            // 重試連線
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                console.log(`嘗試重新連線 (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
                
                setTimeout(() => {
                    this.startConnection();
                }, this.reconnectDelay);
            }
        }
    }

    // 發送私人訊息
    async sendPrivateMessage(targetUserId, message) {
        if (!this.isConnected) {
            throw new Error('SignalR 尚未連線');
        }

        try {
            await this.connection.invoke('SendPrivateMessage', targetUserId, message);
        } catch (error) {
            console.error('發送私人訊息失敗:', error);
            throw error;
        }
    }

    // 加入聊天室
    async joinChatRoom(roomName) {
        if (!this.isConnected) {
            throw new Error('SignalR 尚未連線');
        }

        try {
            await this.connection.invoke('JoinChatRoom', roomName);
        } catch (error) {
            console.error('加入聊天室失敗:', error);
            throw error;
        }
    }

    // 離開聊天室
    async leaveChatRoom(roomName) {
        if (!this.isConnected) {
            throw new Error('SignalR 尚未連線');
        }

        try {
            await this.connection.invoke('LeaveChatRoom', roomName);
        } catch (error) {
            console.error('離開聊天室失敗:', error);
            throw error;
        }
    }

    // 發送聊天室訊息
    async sendRoomMessage(roomName, message) {
        if (!this.isConnected) {
            throw new Error('SignalR 尚未連線');
        }

        try {
            await this.connection.invoke('SendRoomMessage', roomName, message);
        } catch (error) {
            console.error('發送聊天室訊息失敗:', error);
            throw error;
        }
    }

    // 取得線上用戶
    async getOnlineUsers() {
        if (!this.isConnected) {
            throw new Error('SignalR 尚未連線');
        }

        try {
            await this.connection.invoke('GetOnlineUsers');
        } catch (error) {
            console.error('取得線上用戶失敗:', error);
            throw error;
        }
    }

    // 事件監聽器管理
    on(event, handler) {
        if (!this.eventHandlers.has(event)) {
            this.eventHandlers.set(event, []);
        }
        this.eventHandlers.get(event).push(handler);
    }

    off(event, handler) {
        if (this.eventHandlers.has(event)) {
            const handlers = this.eventHandlers.get(event);
            const index = handlers.indexOf(handler);
            if (index > -1) {
                handlers.splice(index, 1);
            }
        }
    }

    emit(event, data) {
        if (this.eventHandlers.has(event)) {
            this.eventHandlers.get(event).forEach(handler => {
                try {
                    handler(data);
                } catch (error) {
                    console.error(`事件處理器錯誤 (${event}):`, error);
                }
            });
        }
    }

    // 輔助方法
    getTokenFromCookie() {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'AuthToken') {
                return value;
            }
        }
        return null;
    }

    // 關閉連線
    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
        }
    }
}

// 全域 SignalR 實例
window.matrixSignalR = new MatrixSignalR();
```

### 2. Vue.js 整合
```javascript
// components/chat.js
export const useChat = () => {
    const messages = Vue.ref([])
    const onlineUsers = Vue.ref([])
    const currentRoom = Vue.ref('')
    const isConnected = Vue.ref(false)

    // 初始化 SignalR 事件
    const initializeSignalR = () => {
        // 連線狀態
        window.matrixSignalR.on('connected', () => {
            isConnected.value = true
        })

        window.matrixSignalR.on('disconnected', () => {
            isConnected.value = false
        })

        // 私人訊息
        window.matrixSignalR.on('privateMessage', (data) => {
            messages.value.push({
                id: data.messageId,
                type: 'private',
                senderId: data.senderId,
                senderName: data.senderName,
                message: data.message,
                timestamp: new Date(data.timestamp),
                isOwn: data.senderId === getCurrentUserId()
            })
        })

        // 聊天室訊息
        window.matrixSignalR.on('roomMessage', (data) => {
            if (data.roomName === currentRoom.value) {
                messages.value.push({
                    type: 'room',
                    roomName: data.roomName,
                    senderId: data.senderId,
                    senderName: data.senderName,
                    message: data.message,
                    timestamp: new Date(data.timestamp),
                    isOwn: data.senderId === getCurrentUserId()
                })
            }
        })

        // 通知
        window.matrixSignalR.on('notification', (data) => {
            showNotification(data.title, data.message, data.type)
        })

        // 用戶狀態
        window.matrixSignalR.on('userOnline', (data) => {
            const existingUser = onlineUsers.value.find(u => u.userId === data.userId)
            if (!existingUser) {
                onlineUsers.value.push({
                    userId: data.userId,
                    userName: data.userName,
                    status: 'online',
                    connectedAt: new Date(data.connectionTime)
                })
            }
        })

        window.matrixSignalR.on('userOffline', (data) => {
            const userIndex = onlineUsers.value.findIndex(u => u.userId === data.userId)
            if (userIndex > -1) {
                onlineUsers.value.splice(userIndex, 1)
            }
        })

        window.matrixSignalR.on('onlineUsers', (users) => {
            onlineUsers.value = users.map(user => ({
                userId: user.userId,
                userName: user.userName,
                status: 'online',
                connectedAt: new Date(user.connectedAt)
            }))
        })
    }

    // 發送私人訊息
    const sendPrivateMessage = async (targetUserId, message) => {
        try {
            await window.matrixSignalR.sendPrivateMessage(targetUserId, message)
        } catch (error) {
            console.error('發送私人訊息失敗:', error)
            throw error
        }
    }

    // 加入聊天室
    const joinRoom = async (roomName) => {
        try {
            await window.matrixSignalR.joinChatRoom(roomName)
            currentRoom.value = roomName
            messages.value = [] // 清空訊息
        } catch (error) {
            console.error('加入聊天室失敗:', error)
            throw error
        }
    }

    // 發送聊天室訊息
    const sendRoomMessage = async (message) => {
        if (!currentRoom.value) {
            throw new Error('尚未加入任何聊天室')
        }

        try {
            await window.matrixSignalR.sendRoomMessage(currentRoom.value, message)
        } catch (error) {
            console.error('發送聊天室訊息失敗:', error)
            throw error
        }
    }

    // 顯示通知
    const showNotification = (title, message, type) => {
        // 實作通知顯示邏輯
        if (Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: '/static/img/Matrix_logo.png'
            })
        }
        
        // 也可以顯示在頁面上的通知區域
        // ...
    }

    // 取得當前用戶 ID
    const getCurrentUserId = () => {
        // 從 JWT Token 或其他方式取得當前用戶 ID
        return 'current-user-id'
    }

    // 初始化
    Vue.onMounted(() => {
        initializeSignalR()
    })

    Vue.onUnmounted(() => {
        if (currentRoom.value) {
            window.matrixSignalR.leaveChatRoom(currentRoom.value)
        }
    })

    return {
        messages: Vue.readonly(messages),
        onlineUsers: Vue.readonly(onlineUsers),
        currentRoom: Vue.readonly(currentRoom),
        isConnected: Vue.readonly(isConnected),
        sendPrivateMessage,
        joinRoom,
        sendRoomMessage
    }
}
```

## 進階功能實作

### 1. 訊息持久化
```csharp
// Services/MessageService.cs
public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<MatrixHub> _hubContext;

    public async Task<Message> CreateMessageAsync(string senderId, string receiverId, string content)
    {
        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = Guid.Parse(senderId),
            ReceiverId = Guid.Parse(receiverId),
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // 即時推送訊息
        await _hubContext.Clients.Group($"user_{receiverId}")
            .SendAsync("ReceivePrivateMessage", new
            {
                MessageId = message.MessageId,
                SenderId = senderId,
                Content = content,
                Timestamp = message.CreatedAt,
                IsRead = false
            });

        return message;
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2, int page = 1, int pageSize = 50)
    {
        var user1Id = Guid.Parse(userId1);
        var user2Id = Guid.Parse(userId2);

        return await _context.Messages
            .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                       (m.SenderId == user2Id && m.ReceiverId == user1Id))
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .ToListAsync();
    }

    public async Task MarkMessagesAsReadAsync(string senderId, string receiverId)
    {
        var senderGuid = Guid.Parse(senderId);
        var receiverGuid = Guid.Parse(receiverId);

        var unreadMessages = await _context.Messages
            .Where(m => m.SenderId == senderGuid && m.ReceiverId == receiverGuid && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // 通知發送者訊息已讀
        await _hubContext.Clients.Group($"user_{senderId}")
            .SendAsync("MessagesRead", new
            {
                ReceiverId = receiverId,
                MessageIds = unreadMessages.Select(m => m.MessageId),
                ReadAt = DateTime.UtcNow
            });
    }
}
```

### 2. 連線狀態管理
```csharp
public class ConnectionManager
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private static readonly ConcurrentDictionary<string, UserInfo> _connectedUsers = new();

    public void AddConnection(string userId, string connectionId, string userName)
    {
        lock (_userConnections)
        {
            _userConnections.AddOrUpdate(userId, new HashSet<string> { connectionId },
                (key, existingConnections) =>
                {
                    existingConnections.Add(connectionId);
                    return existingConnections;
                });
        }

        _connectedUsers.TryAdd(userId, new UserInfo
        {
            UserId = userId,
            UserName = userName,
            LastActivity = DateTime.UtcNow,
            ConnectionCount = _userConnections[userId].Count
        });
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        lock (_userConnections)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                    _connectedUsers.TryRemove(userId, out _);
                }
                else
                {
                    _connectedUsers.TryGetValue(userId, out var userInfo);
                    if (userInfo != null)
                    {
                        userInfo.ConnectionCount = connections.Count;
                    }
                }
            }
        }
    }

    public bool IsUserOnline(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    public IEnumerable<string> GetUserConnections(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) 
            ? connections.ToList() 
            : new List<string>();
    }

    public IEnumerable<UserInfo> GetOnlineUsers()
    {
        return _connectedUsers.Values.ToList();
    }

    public int GetOnlineUserCount()
    {
        return _connectedUsers.Count;
    }

    // 更新用戶活動時間
    public void UpdateUserActivity(string userId)
    {
        if (_connectedUsers.TryGetValue(userId, out var userInfo))
        {
            userInfo.LastActivity = DateTime.UtcNow;
        }
    }

    public class UserInfo
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTime LastActivity { get; set; }
        public int ConnectionCount { get; set; }
    }
}
```

### 3. 群組管理功能
```csharp
public class ChatRoomManager
{
    private static readonly ConcurrentDictionary<string, ChatRoom> _chatRooms = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _roomMembers = new();

    public ChatRoom CreateRoom(string roomName, string creatorId, int maxMembers = 100)
    {
        var room = new ChatRoom
        {
            RoomId = Guid.NewGuid().ToString(),
            RoomName = roomName,
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow,
            MaxMembers = maxMembers,
            IsActive = true
        };

        _chatRooms.TryAdd(room.RoomId, room);
        _roomMembers.TryAdd(room.RoomId, new HashSet<string>());

        return room;
    }

    public bool JoinRoom(string roomId, string userId)
    {
        if (!_chatRooms.TryGetValue(roomId, out var room))
            return false;

        if (!room.IsActive)
            return false;

        if (_roomMembers.TryGetValue(roomId, out var members))
        {
            if (members.Count >= room.MaxMembers)
                return false;

            members.Add(userId);
            room.CurrentMembers = members.Count;
            return true;
        }

        return false;
    }

    public bool LeaveRoom(string roomId, string userId)
    {
        if (_roomMembers.TryGetValue(roomId, out var members))
        {
            members.Remove(userId);
            
            if (_chatRooms.TryGetValue(roomId, out var room))
            {
                room.CurrentMembers = members.Count;
            }

            return true;
        }

        return false;
    }

    public IEnumerable<string> GetRoomMembers(string roomId)
    {
        return _roomMembers.TryGetValue(roomId, out var members) 
            ? members.ToList() 
            : new List<string>();
    }

    public IEnumerable<ChatRoom> GetActiveRooms()
    {
        return _chatRooms.Values.Where(r => r.IsActive).ToList();
    }

    public class ChatRoom
    {
        public string RoomId { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string CreatorId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int MaxMembers { get; set; }
        public int CurrentMembers { get; set; }
        public bool IsActive { get; set; }
    }
}
```

---

**建立日期**: 2025-08-29  
**適用版本**: SignalR Core 8.0  
**相關檔案**: Hubs/MatrixHub.cs, wwwroot/js/signalr/matrix-signalr.js  
**傳輸協議**: WebSockets, Server-Sent Events, Long Polling  
**學習資源**: [SignalR 官方文檔](https://docs.microsoft.com/aspnet/core/signalr/), [JavaScript 客戶端](https://docs.microsoft.com/aspnet/core/signalr/javascript-client)