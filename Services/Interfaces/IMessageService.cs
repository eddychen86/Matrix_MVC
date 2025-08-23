using Matrix.Models;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 聊天訊息服務介面
    /// 定義聊天訊息相關的業務邏輯操作
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// 發送訊息
        /// </summary>
        /// <param name="senderId">發送者ID</param>
        /// <param name="receiverId">接收者ID</param>
        /// <param name="content">訊息內容</param>
        /// <returns>發送的訊息物件</returns>
        Task<Message> SendMessageAsync(Guid senderId, Guid receiverId, string content);

        /// <summary>
        /// 取得兩個用戶之間的聊天記錄
        /// </summary>
        /// <param name="user1Id">用戶1 ID</param>
        /// <param name="user2Id">用戶2 ID</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>聊天記錄列表</returns>
        Task<IEnumerable<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得用戶的聊天列表（最近聊天的對象）
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="limit">返回數量限制</param>
        /// <returns>最近聊天記錄</returns>
        Task<IEnumerable<Message>> GetRecentChatsAsync(Guid userId, int limit = 10);

        /// <summary>
        /// 取得用戶的未讀訊息數量
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>未讀訊息數量</returns>
        Task<int> GetUnreadMessageCountAsync(Guid userId);

        /// <summary>
        /// 取得用戶與特定發送者的未讀訊息數量
        /// </summary>
        /// <param name="userId">接收者ID</param>
        /// <param name="senderId">發送者ID</param>
        /// <returns>未讀訊息數量</returns>
        Task<int> GetUnreadMessageCountFromSenderAsync(Guid userId, Guid senderId);

        /// <summary>
        /// 將訊息標記為已讀
        /// </summary>
        /// <param name="messageId">訊息ID</param>
        /// <param name="userId">執行操作的用戶ID（驗證權限用）</param>
        /// <returns>操作是否成功</returns>
        Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId);

        /// <summary>
        /// 將用戶與特定發送者的所有未讀訊息標記為已讀
        /// </summary>
        /// <param name="userId">接收者ID</param>
        /// <param name="senderId">發送者ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> MarkConversationAsReadAsync(Guid userId, Guid senderId);

        /// <summary>
        /// 搜尋用戶的聊天訊息
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="keyword">搜尋關鍵字</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>符合條件的訊息列表</returns>
        Task<IEnumerable<Message>> SearchMessagesAsync(Guid userId, string keyword, int page = 1, int pageSize = 20);

        /// <summary>
        /// 刪除訊息
        /// </summary>
        /// <param name="messageId">訊息ID</param>
        /// <param name="userId">執行操作的用戶ID（驗證權限用）</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);

        /// <summary>
        /// 刪除兩個用戶之間的所有聊天記錄
        /// </summary>
        /// <param name="userId">執行操作的用戶ID</param>
        /// <param name="otherUserId">對方用戶ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId);

        /// <summary>
        /// 檢查用戶是否有權限查看特定訊息
        /// </summary>
        /// <param name="messageId">訊息ID</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>是否有權限</returns>
        Task<bool> CanUserAccessMessageAsync(Guid messageId, Guid userId);

        /// <summary>
        /// 檢查兩個用戶是否可以互相發送訊息（考慮好友關係、隱私設定等）
        /// </summary>
        /// <param name="senderId">發送者ID</param>
        /// <param name="receiverId">接收者ID</param>
        /// <returns>是否可以發送訊息</returns>
        Task<bool> CanSendMessageAsync(Guid senderId, Guid receiverId);

        /// <summary>
        /// 取得訊息詳細資訊（包含發送者資訊）
        /// </summary>
        /// <param name="messageId">訊息ID</param>
        /// <param name="userId">執行操作的用戶ID（驗證權限用）</param>
        /// <returns>訊息詳細資訊，如果無權限或不存在則返回 null</returns>
        Task<Message?> GetMessageDetailsAsync(Guid messageId, Guid userId);
    }
}