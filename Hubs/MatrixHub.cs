using Microsoft.AspNetCore.SignalR;
using Matrix.Extensions;
using Matrix.Repository.Interfaces;
using System.Security.Claims;

namespace Matrix.Hubs
{
    /// <summary>
    /// Matrix 專案通用 SignalR Hub
    /// 處理所有即時更新功能：通知、功能開關、按讚、追蹤等
    /// </summary>
    public class MatrixHub : Hub
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<MatrixHub> _logger;

        public MatrixHub(IPersonRepository personRepository, ILogger<MatrixHub> logger)
        {
            _personRepository = personRepository;
            _logger = logger;
        }

        /// <summary>
        /// 連接時自動執行：根據用戶身份加入對應群組
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var auth = Context.GetHttpContext()?.GetAuthInfo();

            if (auth != null && auth.IsAuthenticated && auth.UserId != Guid.Empty)
            {
                try
                {
                    // 查詢用戶資料
                    var person = await _personRepository.GetByUserIdAsync(auth.UserId);
                    if (person != null)
                    {
                        // 加入個人群組（用於個人通知）
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{person.PersonId}");

                        // 加入全體用戶群組（用於系統公告）
                        await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");

                        // 管理員額外加入管理員群組
                        if (auth.Role >= 1)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                        }

                        _logger.LogInformation("User {PersonId} connected to MatrixHub", person.PersonId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding user to groups on connection");
                }
            }
            else
            {
                // 訪客加入訪客群組
                await Groups.AddToGroupAsync(Context.ConnectionId, "Guests");
                _logger.LogInformation("Guest user connected to MatrixHub");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 斷線時自動執行
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "User disconnected from MatrixHub with error");
            }
            else
            {
                _logger.LogInformation("User disconnected from MatrixHub");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 手動加入特定群組（可選功能）
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// 手動離開特定群組（可選功能）
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// 心跳檢測（保持連接活躍）
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        /// <summary>
        /// 通知新貼文發布（廣播給所有用戶，除了發文者）
        /// </summary>
        public async Task NotifyNewPost(object postData)
        {
            try
            {
                var auth = Context.GetHttpContext()?.GetAuthInfo();
                if (auth != null && auth.IsAuthenticated)
                {
                    var person = await _personRepository.GetByUserIdAsync(auth.UserId);
                    if (person != null)
                    {
                        // 廣播給所有用戶，但排除發文者自己
                        await Clients.GroupExcept("AllUsers", Context.ConnectionId)
                            .SendAsync("NewPostReceived", postData);

                        _logger.LogInformation("New post notification sent by user {PersonId}", person.PersonId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new post notification");
            }
        }
    }
}