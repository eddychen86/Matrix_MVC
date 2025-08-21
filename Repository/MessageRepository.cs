using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Data;

namespace Matrix.Repository
{
    /// <summary>
    /// 聊天訊息資料存取實作
    /// </summary>
    public class MessageRepository : BaseRepository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetBySentIdAsync(Guid sentId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetUnreadCountAsync(Guid receiverId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetUnreadCountFromSenderAsync(Guid receiverId, Guid senderId)
        {
            throw new NotImplementedException();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            throw new NotImplementedException();
        }

        public async Task MarkConversationAsReadAsync(Guid receiverId, Guid senderId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Message>> GetRecentConversationsAsync(Guid userId, int limit = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(Guid userId, string keyword, int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteConversationAsync(Guid user1Id, Guid user2Id)
        {
            throw new NotImplementedException();
        }

        public async Task<Message?> GetMessageWithSenderAsync(Guid messageId)
        {
            throw new NotImplementedException();
        }
    }
}