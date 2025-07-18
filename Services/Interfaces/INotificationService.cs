using Matrix.DTOs;

namespace Matrix.Services.Interfaces;

/// <summary>
/// 通知服務介面
/// 定義通知相關的業務邏輯操作
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 根據 ID 獲取通知資料
    /// </summary>
    /// <param name="id">通知 ID</param>
    /// <returns>通知資料傳輸物件，如果不存在則返回 null</returns>
    Task<NotificationDto?> GetNotificationAsync(Guid id);

    /// <summary>
    /// 獲取使用者的通知列表
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁數量</param>
    /// <param name="type">通知類型</param>
    /// <param name="isRead">是否已讀</param>
    /// <returns>通知列表和總數量</returns>
    Task<(List<NotificationDto> Notifications, int TotalCount)> GetNotificationsAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        int? type = null, 
        int? isRead = null);

    /// <summary>
    /// 發送系統通知
    /// </summary>
    /// <param name="receiverId">接收者 ID</param>
    /// <param name="title">通知標題</param>
    /// <param name="content">通知內容</param>
    /// <param name="type">通知類型</param>
    /// <returns>發送是否成功</returns>
    Task<bool> SendSystemNotificationAsync(Guid receiverId, string title, string content, int type = 0);

    /// <summary>
    /// 發送使用者通知
    /// </summary>
    /// <param name="senderId">發送者 ID</param>
    /// <param name="receiverId">接收者 ID</param>
    /// <param name="type">通知類型</param>
    /// <param name="relatedId">相關實體 ID</param>
    /// <returns>發送是否成功</returns>
    Task<bool> SendUserNotificationAsync(Guid senderId, Guid receiverId, int type, Guid? relatedId = null);

    /// <summary>
    /// 標記通知為已讀
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// 標記通知為未讀
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkAsUnreadAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// 批量標記通知為已讀
    /// </summary>
    /// <param name="notificationIds">通知 ID 列表</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId);

    /// <summary>
    /// 標記所有通知為已讀
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkAllAsReadAsync(Guid userId);

    /// <summary>
    /// 刪除通知
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>刪除是否成功</returns>
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// 批量刪除通知
    /// </summary>
    /// <param name="notificationIds">通知 ID 列表</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>刪除是否成功</returns>
    Task<bool> DeleteMultipleNotificationsAsync(List<Guid> notificationIds, Guid userId);

    /// <summary>
    /// 獲取未讀通知數量
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>未讀通知數量</returns>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// 獲取通知統計資料
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>通知統計資料</returns>
    Task<Dictionary<string, int>> GetNotificationStatsAsync(Guid userId);

    /// <summary>
    /// 清理過期通知
    /// </summary>
    /// <param name="days">保留天數</param>
    /// <returns>清理的通知數量</returns>
    Task<int> CleanupOldNotificationsAsync(int days = 30);
}