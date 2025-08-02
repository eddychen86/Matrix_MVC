using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 通知資料存取介面
    /// </summary>
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>根據接收者ID取得通知列表</summary>
        Task<IEnumerable<Notification>> GetByReceiverIdAsync(Guid receiverId, int page = 1, int pageSize = 20);

        /// <summary>取得未讀通知</summary>
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid receiverId);

        /// <summary>標記通知為已讀</summary>
        Task MarkAsReadAsync(Guid notificationId);

        /// <summary>批量標記通知為已讀</summary>
        Task MarkMultipleAsReadAsync(IEnumerable<Guid> notificationIds);

        /// <summary>標記用戶所有通知為已讀</summary>
        Task MarkAllAsReadAsync(Guid receiverId);

        /// <summary>計算未讀通知數量</summary>
        Task<int> CountUnreadNotificationsAsync(Guid receiverId);

        /// <summary>根據類型取得通知</summary>
        Task<IEnumerable<Notification>> GetByTypeAsync(Guid receiverId, int type, int page = 1, int pageSize = 20);

        /// <summary>刪除過期通知</summary>
        Task DeleteExpiredNotificationsAsync(DateTime expiredBefore);
    }
}