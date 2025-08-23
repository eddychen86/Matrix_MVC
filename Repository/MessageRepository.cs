using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Data;

namespace Matrix.Repository
{
    /// <summary>
    /// 聊天訊息資料存取實作
    /// </summary>
    public class MessageRepository(ApplicationDbContext context)
        : BaseRepository<Message>(context), IMessageRepository
    {
        public async Task<IEnumerable<Message>> GetBySentIdAsync(Guid sentId)
        {
            return await _dbSet
                .Where(m => m.SentId == sentId)
                .OrderBy(m => m.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == receiverId)
                .OrderByDescending(m => m.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(m => (m.SentId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SentId == user2Id && m.ReceiverId == user1Id))
                .OrderByDescending(m => m.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.User)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid receiverId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == receiverId && m.IsRead == 0)
                .CountAsync();
        }

        public async Task<int> GetUnreadCountFromSenderAsync(Guid receiverId, Guid senderId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == receiverId && m.SentId == senderId && m.IsRead == 0)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await _dbSet.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = 1;
            }
        }

        public async Task MarkConversationAsReadAsync(Guid receiverId, Guid senderId)
        {
            var unreadMessages = await _dbSet
                .Where(m => m.ReceiverId == receiverId && m.SentId == senderId && m.IsRead == 0)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = 1;
            }
        }

        public async Task<IEnumerable<Message>> GetRecentConversationsAsync(Guid userId, int limit = 10)
        {
            return await _dbSet
                .Where(m => m.SentId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SentId == userId ? m.ReceiverId : m.SentId)
                .Select(g => g.OrderByDescending(m => m.CreateTime).First())
                .OrderByDescending(m => m.CreateTime)
                .Take(limit)
                .Include(m => m.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(Guid userId, string keyword, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(m => (m.SentId == userId || m.ReceiverId == userId) &&
                           m.Content.Contains(keyword))
                .OrderByDescending(m => m.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.User)
                .ToListAsync();
        }

        public async Task DeleteConversationAsync(Guid user1Id, Guid user2Id)
        {
            var messages = await _dbSet
                .Where(m => (m.SentId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SentId == user2Id && m.ReceiverId == user1Id))
                .ToListAsync();

            _dbSet.RemoveRange(messages);
        }

        public async Task<Message?> GetMessageWithSenderAsync(Guid messageId)
        {
            return await _dbSet
                .Where(m => m.MsgId == messageId)
                .Include(m => m.User)
                .FirstOrDefaultAsync();
        }
    }
}