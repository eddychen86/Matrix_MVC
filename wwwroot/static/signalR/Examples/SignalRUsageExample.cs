using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Examples
{
    /// <summary>
    /// SignalR 使用範例
    /// 示範如何在 Controller 和 Service 中使用 SignalR 功能
    /// </summary>
    public class SignalRUsageExample
    {
        private readonly ISignalRService _signalRService;

        public SignalRUsageExample(ISignalRService signalRService)
        {
            _signalRService = signalRService;
        }

        #region 功能開關使用範例

        /// <summary>
        /// 範例：在後台設定中切換功能開關
        /// </summary>
        public async Task ToggleFeatureExample(string featureName, bool isEnabled)
        {
            // 更新資料庫設定（這裡省略）
            // await _systemSettings.SetValueAsync($"Enable{featureName}", isEnabled.ToString());

            // 廣播給所有用戶
            await _signalRService.BroadcastFeatureToggleAsync(featureName, isEnabled);
        }

        /// <summary>
        /// 範例：發送系統維護公告
        /// </summary>
        public async Task SendMaintenanceAnnouncementExample()
        {
            await _signalRService.BroadcastSystemAnnouncementAsync(
                "系統維護通知",
                "系統將於今晚 23:00 進行維護，預計維護時間 30 分鐘。",
                "warning"
            );
        }

        #endregion

        #region 社交互動使用範例

        /// <summary>
        /// 範例：處理按讚動作
        /// </summary>
        public async Task HandlePraiseExample(Guid articleId, Guid userId, Guid authorId)
        {
            // 更新資料庫（這裡省略）
            // var newCount = await _praiseService.TogglePraiseAsync(userId, articleId);

            var newCount = 42; // 假設的新數量

            // 廣播按讚更新，同時通知文章作者
            await _signalRService.BroadcastInteractionUpdateAsync(
                articleId, 
                "praise", 
                newCount, 
                authorId // 文章作者會收到個人通知
            );
        }

        /// <summary>
        /// 範例：處理追蹤動作
        /// </summary>
        public async Task HandleFollowExample(Guid followerId, Guid followedId, bool isFollowing)
        {
            // 更新資料庫（這裡省略）
            // await _followService.ToggleFollowAsync(followerId, followedId);

            // 廣播追蹤更新
            await _signalRService.BroadcastFollowUpdateAsync(followerId, followedId, isFollowing);
        }

        #endregion

        #region 通知使用範例

        /// <summary>
        /// 範例：發送個人通知
        /// </summary>
        public async Task SendPersonalNotificationExample(Guid personId)
        {
            var notification = new
            {
                type = "system",
                title = "歡迎使用 Matrix！",
                message = "感謝您註冊 Matrix，開始探索精彩內容吧！",
                timestamp = DateTime.UtcNow,
                actionUrl = "/home"
            };

            await _signalRService.SendPersonalNotificationAsync(personId, notification);
        }

        /// <summary>
        /// 範例：批量發送通知
        /// </summary>
        public async Task SendBulkNotificationExample(List<Guid> personIds)
        {
            var notification = new
            {
                type = "announcement",
                title = "新功能上線！",
                message = "我們新增了即時通知功能，現在您可以即時收到最新動態。",
                timestamp = DateTime.UtcNow
            };

            await _signalRService.SendNotificationToUsersAsync(personIds, notification);
        }

        #endregion

        #region 管理員功能使用範例

        /// <summary>
        /// 範例：發送管理員通知
        /// </summary>
        public async Task SendAdminAlertExample()
        {
            var alert = new
            {
                type = "security",
                title = "安全警告",
                message = "檢測到異常登入嘗試，請檢查安全日誌。",
                timestamp = DateTime.UtcNow,
                severity = "high"
            };

            await _signalRService.SendAdminNotificationAsync(alert);
        }

        /// <summary>
        /// 範例：更新即時統計
        /// </summary>
        public async Task UpdateRealTimeStatsExample()
        {
            var stats = new
            {
                onlineUsers = 150,
                totalPosts = 2340,
                todayRegistrations = 12,
                serverLoad = 45.2
            };

            await _signalRService.BroadcastStatsUpdateAsync(stats);
        }

        #endregion
    }

    /// <summary>
    /// 在 Controller 中使用 SignalR 的範例
    /// </summary>
    [ApiController]
    [Route("api/examples")]
    public class SignalRExampleController : ControllerBase
    {
        private readonly ISignalRService _signalRService;

        public SignalRExampleController(ISignalRService signalRService)
        {
            _signalRService = signalRService;
        }

        /// <summary>
        /// 測試廣播功能
        /// </summary>
        [HttpPost("test-broadcast")]
        public async Task<IActionResult> TestBroadcast([FromBody] TestBroadcastRequest request)
        {
            await _signalRService.BroadcastToAllAsync("TestMessage", new
            {
                message = request.Message,
                timestamp = DateTime.UtcNow,
                sender = "System"
            });

            return Ok(new { success = true, message = "廣播已發送" });
        }

        /// <summary>
        /// 測試個人通知
        /// </summary>
        [HttpPost("test-personal-notification/{personId}")]
        public async Task<IActionResult> TestPersonalNotification(Guid personId, [FromBody] TestNotificationRequest request)
        {
            await _signalRService.SendPersonalNotificationAsync(personId, new
            {
                title = request.Title,
                message = request.Message,
                type = request.Type ?? "info",
                timestamp = DateTime.UtcNow
            });

            return Ok(new { success = true, message = "個人通知已發送" });
        }
    }

    #region DTO 類別

    public class TestBroadcastRequest
    {
        public string Message { get; set; } = "";
    }

    public class TestNotificationRequest
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Type { get; set; }
    }

    #endregion
}