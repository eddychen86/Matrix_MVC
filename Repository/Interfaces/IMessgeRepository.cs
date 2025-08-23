using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 聊天訊息資料存取介面
    /// </summary>
    public interface IMessageRepository : IRepository<Message>
    {
        /// <summary>根據發送者ID取得發送的訊息</summary>
        Task<IEnumerable<Message>> GetBySentIdAsync(Guid sentId);

        /// <summary>根據接收者ID取得接收的訊息</summary>
        Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId);

        /// <summary>取得兩個用戶之間的聊天記錄</summary>
        Task<IEnumerable<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 20);

        /// <summary>取得用戶的未讀訊息數量</summary>
        Task<int> GetUnreadCountAsync(Guid receiverId);

        /// <summary>取得用戶與特定發送者的未讀訊息數量</summary>
        Task<int> GetUnreadCountFromSenderAsync(Guid receiverId, Guid senderId);

        /// <summary>將訊息標記為已讀</summary>
        Task MarkAsReadAsync(Guid messageId);

        /// <summary>將用戶與特定發送者的所有訊息標記為已讀</summary>
        Task MarkConversationAsReadAsync(Guid receiverId, Guid senderId);

        /// <summary>取得用戶的聊天對話列表（最近聊天的對象）</summary>
        Task<IEnumerable<Message>> GetRecentConversationsAsync(Guid userId, int limit = 10);

        /// <summary>搜尋包含關鍵字的訊息</summary>
        Task<IEnumerable<Message>> SearchMessagesAsync(Guid userId, string keyword, int page = 1, int pageSize = 20);

        /// <summary>刪除兩個用戶之間的所有聊天記錄</summary>
        Task DeleteConversationAsync(Guid user1Id, Guid user2Id);

        /// <summary>取得訊息詳細資訊（包含發送者資訊）</summary>
        Task<Message?> GetMessageWithSenderAsync(Guid messageId);
    }
}