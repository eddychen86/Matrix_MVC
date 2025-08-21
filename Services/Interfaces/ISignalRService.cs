namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// SignalR 即時通訊服務介面
    /// </summary>
    public interface ISignalRService
    {
        #region 功能開關相關
        /// <summary>
        /// 廣播功能開關變更給所有用戶
        /// </summary>
        Task BroadcastFeatureToggleAsync(string featureName, bool isEnabled);

        /// <summary>
        /// 發送系統公告給所有用戶
        /// </summary>
        Task BroadcastSystemAnnouncementAsync(string title, string message, string type = "info");
        #endregion

        #region 通知相關
        /// <summary>
        /// 發送個人通知
        /// </summary>
        Task SendPersonalNotificationAsync(Guid personId, object notification);

        /// <summary>
        /// 發送通知給多個用戶
        /// </summary>
        Task SendNotificationToUsersAsync(IEnumerable<Guid> personIds, object notification);
        #endregion

        #region 社交互動相關
        /// <summary>
        /// 廣播按讚/收藏狀態更新
        /// </summary>
        Task BroadcastInteractionUpdateAsync(Guid articleId, string action, int newCount, Guid? targetUserId = null);

        /// <summary>
        /// 廣播追蹤狀態更新
        /// </summary>
        Task BroadcastFollowUpdateAsync(Guid followerId, Guid followedId, bool isFollowing);
        #endregion

        #region 管理員功能
        /// <summary>
        /// 發送管理員通知
        /// </summary>
        Task SendAdminNotificationAsync(object notification);

        /// <summary>
        /// 廣播網站統計更新（給管理員）
        /// </summary>
        Task BroadcastStatsUpdateAsync(object stats);
        #endregion

        #region 通用方法
        /// <summary>
        /// 發送自定義事件給指定群組
        /// </summary>
        Task SendToGroupAsync(string groupName, string eventName, object data);

        /// <summary>
        /// 廣播給所有連接的用戶
        /// </summary>
        Task BroadcastToAllAsync(string eventName, object data);
        #endregion
    }
}