using Microsoft.AspNetCore.SignalR;
using Matrix.Hubs;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// SignalR 即時通訊服務
    /// 封裝所有 SignalR 相關的業務邏輯
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<MatrixHub> _hubContext;
        private readonly ILogger<SignalRService> _logger;

        public SignalRService(IHubContext<MatrixHub> hubContext, ILogger<SignalRService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        #region 功能開關相關

        /// <summary>
        /// 廣播功能開關變更給所有用戶
        /// </summary>
        public async Task BroadcastFeatureToggleAsync(string featureName, bool isEnabled)
        {
            try
            {
                await _hubContext.Clients.Group("AllUsers").SendAsync("FeatureToggleChanged", featureName, isEnabled);
                _logger.LogInformation("Broadcasted feature toggle: {FeatureName} = {IsEnabled}", featureName, isEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting feature toggle");
            }
        }

        /// <summary>
        /// 發送系統公告給所有用戶
        /// </summary>
        public async Task BroadcastSystemAnnouncementAsync(string title, string message, string type = "info")
        {
            try
            {
                var announcement = new
                {
                    title,
                    message,
                    type, // info, warning, error, success
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group("AllUsers").SendAsync("SystemAnnouncement", announcement);
                _logger.LogInformation("Broadcasted system announcement: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system announcement");
            }
        }

        #endregion

        #region 通知相關

        /// <summary>
        /// 發送個人通知
        /// </summary>
        public async Task SendPersonalNotificationAsync(Guid personId, object notification)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{personId}").SendAsync("NewNotification", notification);
                _logger.LogInformation("Sent personal notification to user {PersonId}", personId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending personal notification to user {PersonId}", personId);
            }
        }

        /// <summary>
        /// 發送通知給多個用戶
        /// </summary>
        public async Task SendNotificationToUsersAsync(IEnumerable<Guid> personIds, object notification)
        {
            try
            {
                var groupNames = personIds.Select(id => $"User_{id}").ToList();
                await _hubContext.Clients.Groups(groupNames).SendAsync("NewNotification", notification);
                _logger.LogInformation("Sent notification to {Count} users", personIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to multiple users");
            }
        }

        #endregion

        #region 社交互動相關

        /// <summary>
        /// 廣播按讚/收藏狀態更新
        /// </summary>
        public async Task BroadcastInteractionUpdateAsync(Guid articleId, string action, int newCount, Guid? targetUserId = null)
        {
            try
            {
                var update = new
                {
                    articleId,
                    action, // "praise", "collect"
                    newCount,
                    timestamp = DateTime.UtcNow
                };

                // 廣播給所有用戶（文章頁面即時更新數量）
                await _hubContext.Clients.Group("AllUsers").SendAsync("InteractionUpdate", update);

                // 如果有目標用戶（文章作者），發送個人通知
                if (targetUserId.HasValue && targetUserId.Value != Guid.Empty)
                {
                    var personalNotification = new
                    {
                        type = action,
                        articleId,
                        message = action == "praise" ? "有人讚了你的文章" : "有人收藏了你的文章",
                        timestamp = DateTime.UtcNow
                    };
                    
                    await SendPersonalNotificationAsync(targetUserId.Value, personalNotification);
                }

                _logger.LogInformation("Broadcasted interaction update: {Action} on article {ArticleId}", action, articleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting interaction update");
            }
        }

        /// <summary>
        /// 廣播追蹤狀態更新
        /// </summary>
        public async Task BroadcastFollowUpdateAsync(Guid followerId, Guid followedId, bool isFollowing)
        {
            try
            {
                var update = new
                {
                    followerId,
                    followedId,
                    isFollowing,
                    timestamp = DateTime.UtcNow
                };

                // 通知被追蹤者
                var notification = new
                {
                    type = "follow",
                    followerId,
                    message = isFollowing ? "有人開始追蹤你" : "有人取消追蹤你",
                    timestamp = DateTime.UtcNow
                };

                await SendPersonalNotificationAsync(followedId, notification);
                
                // 廣播給相關用戶更新追蹤狀態
                await _hubContext.Clients.Groups($"User_{followerId}", $"User_{followedId}")
                    .SendAsync("FollowUpdate", update);

                _logger.LogInformation("Broadcasted follow update: {FollowerId} -> {FollowedId} = {IsFollowing}", 
                    followerId, followedId, isFollowing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting follow update");
            }
        }

        #endregion

        #region 管理員功能

        /// <summary>
        /// 發送管理員通知
        /// </summary>
        public async Task SendAdminNotificationAsync(object notification)
        {
            try
            {
                await _hubContext.Clients.Group("Admins").SendAsync("AdminNotification", notification);
                _logger.LogInformation("Sent admin notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notification");
            }
        }

        /// <summary>
        /// 廣播網站統計更新（給管理員）
        /// </summary>
        public async Task BroadcastStatsUpdateAsync(object stats)
        {
            try
            {
                await _hubContext.Clients.Group("Admins").SendAsync("StatsUpdate", stats);
                _logger.LogInformation("Broadcasted stats update to admins");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting stats update");
            }
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 發送自定義事件給指定群組
        /// </summary>
        public async Task SendToGroupAsync(string groupName, string eventName, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(eventName, data);
                _logger.LogInformation("Sent custom event {EventName} to group {GroupName}", eventName, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending custom event to group");
            }
        }

        /// <summary>
        /// 廣播給所有連接的用戶
        /// </summary>
        public async Task BroadcastToAllAsync(string eventName, object data)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(eventName, data);
                _logger.LogInformation("Broadcasted event {EventName} to all users", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting to all users");
            }
        }

        #endregion
    }
}