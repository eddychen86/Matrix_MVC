using Microsoft.Extensions.Logging;
using Matrix.Models;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 聊天訊息服務實作
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepository,
            IPersonRepository personRepository,
            IFriendshipRepository friendshipRepository,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _personRepository = personRepository;
            _friendshipRepository = friendshipRepository;
            _logger = logger;
        }

        public async Task<Message> SendMessageAsync(Guid senderId, Guid receiverId, string content)
        {
            try
            {
                // 檢查發送者和接收者是否存在
                var sender = await _personRepository.GetByUserIdAsync(senderId);
                var receiver = await _personRepository.GetByUserIdAsync(receiverId);

                if (sender == null)
                {
                    _logger.LogWarning($"Sender with ID {senderId} not found");
                    throw new ArgumentException("發送者不存在", nameof(senderId));
                }

                if (receiver == null)
                {
                    _logger.LogWarning($"Receiver with ID {receiverId} not found");
                    throw new ArgumentException("接收者不存在", nameof(receiverId));
                }

                // 檢查是否可以發送訊息
                if (!await CanSendMessageAsync(senderId, receiverId))
                {
                    _logger.LogWarning($"User {senderId} cannot send message to {receiverId}");
                    throw new UnauthorizedAccessException("沒有權限發送訊息給此用戶");
                }

                // 驗證訊息內容
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new ArgumentException("訊息內容不能為空", nameof(content));
                }

                if (content.Length > 300)
                {
                    throw new ArgumentException("訊息內容不能超過 300 個字元", nameof(content));
                }

                // 創建訊息
                var message = new Message
                {
                    SentId = sender.PersonId,
                    ReceiverId = receiver.PersonId,
                    Content = content,
                    CreateTime = DateTime.Now,
                    IsRead = 0 // 未讀
                };

                var createdMessage = await _messageRepository.AddAsync(message);
                await _messageRepository.SaveChangesAsync();

                _logger.LogInformation($"Message sent from {senderId} to {receiverId}");
                return createdMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message from {senderId} to {receiverId}");
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 20)
        {
            try
            {
                // 檢查用戶是否有權限查看對話
                //if (!await CanUserAccessConversationAsync(user1Id, user2Id))
                //{
                //    _logger.LogWarning($"User {user1Id} cannot access conversation with {user2Id}");
                //    throw new UnauthorizedAccessException("沒有權限查看此對話");
                //}

                // 將 UserId 轉換為 PersonId
                var user1Person = await _personRepository.GetByUserIdAsync(user1Id);
                var user2Person = await _personRepository.GetByUserIdAsync(user2Id);

                if (user1Person == null || user2Person == null)
                {
                    // 如果找不到對應的 Person，返回空列表
                    return Enumerable.Empty<Message>();
                }

                return await _messageRepository.GetConversationAsync(user1Person.PersonId, user2Person.PersonId, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation between {user1Id} and {user2Id}");
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetRecentChatsAsync(Guid userId, int limit = 10)
        {
            try
            {
                // 檢查用戶是否存在
                var user = await _personRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("用戶不存在", nameof(userId));
                }

                return await _messageRepository.GetRecentConversationsAsync(userId, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recent chats for user {userId}");
                throw;
            }
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid userId)
        {
            try
            {
                return await _messageRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread count for user {userId}");
                throw;
            }
        }

        public async Task<int> GetUnreadMessageCountFromSenderAsync(Guid userId, Guid senderId)
        {
            try
            {
                return await _messageRepository.GetUnreadCountFromSenderAsync(userId, senderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread count from {senderId} for user {userId}");
                throw;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId)
        {
            try
            {
                // 檢查用戶是否有權限標記此訊息
                if (!await CanUserAccessMessageAsync(messageId, userId))
                {
                    _logger.LogWarning($"User {userId} cannot access message {messageId}");
                    return false;
                }

                await _messageRepository.MarkAsReadAsync(messageId);
                await _messageRepository.SaveChangesAsync();

                _logger.LogInformation($"Message {messageId} marked as read by user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message {messageId} as read by user {userId}");
                return false;
            }
        }

        public async Task<bool> MarkConversationAsReadAsync(Guid userId, Guid senderId)
        {
            try
            {
                await _messageRepository.MarkConversationAsReadAsync(userId, senderId);
                await _messageRepository.SaveChangesAsync();

                _logger.LogInformation($"Conversation between {userId} and {senderId} marked as read");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking conversation as read between {userId} and {senderId}");
                return false;
            }
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(Guid userId, string keyword, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return new List<Message>();
                }

                return await _messageRepository.SearchMessagesAsync(userId, keyword, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching messages for user {userId} with keyword '{keyword}'");
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                // 檢查訊息是否存在且用戶有權限刪除
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    _logger.LogWarning($"Message {messageId} not found");
                    return false;
                }

                // 只有發送者可以刪除訊息
                if (message.SentId != userId)
                {
                    _logger.LogWarning($"User {userId} cannot delete message {messageId} (not sender)");
                    return false;
                }

                await _messageRepository.DeleteAsync(message);
                await _messageRepository.SaveChangesAsync();

                _logger.LogInformation($"Message {messageId} deleted by user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId} by user {userId}");
                return false;
            }
        }

        public async Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId)
        {
            try
            {
                await _messageRepository.DeleteConversationAsync(userId, otherUserId);
                await _messageRepository.SaveChangesAsync();

                _logger.LogInformation($"Conversation deleted between {userId} and {otherUserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting conversation between {userId} and {otherUserId}");
                return false;
            }
        }

        public async Task<bool> CanUserAccessMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    return false;
                }

                // 用戶只能查看自己發送或接收的訊息
                return message.SentId == userId || message.ReceiverId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking access for message {messageId} by user {userId}");
                return false;
            }
        }

        public async Task<bool> CanSendMessageAsync(Guid senderId, Guid receiverId)
        {
            try
            {
                // 不能發送訊息給自己
                if (senderId == receiverId)
                {
                    return false;
                }

                // 檢查接收者是否為私人帳號
                var receiver = await _personRepository.GetByUserIdAsync(receiverId);
                if (receiver == null)
                {
                    return false;
                }

                // 如果接收者是公開帳號，可以發送
                if (receiver.IsPrivate == 0)
                {
                    return true;
                }

                // 如果接收者是私人帳號，檢查是否為好友
                var friendship = await _friendshipRepository.GetAsync(f => 
                    (f.UserId == senderId && f.FriendId == receiverId && f.Status == FriendshipStatus.Accepted) ||
                    (f.UserId == receiverId && f.FriendId == senderId && f.Status == FriendshipStatus.Accepted));

                return friendship != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user {senderId} can send message to {receiverId}");
                return false;
            }
        }

        public async Task<Message?> GetMessageDetailsAsync(Guid messageId, Guid userId)
        {
            try
            {
                // 檢查權限
                if (!await CanUserAccessMessageAsync(messageId, userId))
                {
                    return null;
                }

                return await _messageRepository.GetMessageWithSenderAsync(messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message details {messageId} for user {userId}");
                return null;
            }
        }

        /// <summary>
        /// 檢查用戶是否可以查看與另一個用戶的對話
        /// </summary>
        private async Task<bool> CanUserAccessConversationAsync(Guid userId, Guid otherUserId)
        {
            try
            {
                // 先將 UserId 轉換為 PersonId
                var userPerson = await _personRepository.GetByUserIdAsync(userId);
                var otherUserPerson = await _personRepository.GetByUserIdAsync(otherUserId);

                if (userPerson == null || otherUserPerson == null)
                {
                    return false; // 如果任一用戶找不到對應的 Person，則無法查看
                }

                // 使用 PersonId 進行查詢
                var hasMessage = await _messageRepository.ExistsAsync(m =>
                    (m.SentId == userPerson.PersonId && m.ReceiverId == otherUserPerson.PersonId) ||
                    (m.SentId == otherUserPerson.PersonId && m.ReceiverId == userPerson.PersonId));

                return hasMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking conversation access between {userId} and {otherUserId}");
                return false;
            }
        }
    }
}