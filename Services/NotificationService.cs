namespace Matrix.Services
{
    /// <summary>
    /// 通知服務類別 - 處理通知相關業務邏輯
    /// </summary>
    public class NotificationService(ApplicationDbContext _context) : INotificationService
    {
        /// <summary>
        /// 根據 ID 獲取通知資料
        /// </summary>
        public async Task<NotificationDto?> GetNotificationAsync(Guid id)
        {
            var notification = await _context.Notifications
                .AsNoTracking() // 只讀查詢
                .Include(n => n.Receiver)
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(n => n.NotifyId == id);

            return notification == null ? null : MapToNotificationDto(notification, true);
        }

        /// <summary>
        /// 獲取使用者的通知列表
        /// </summary>
        public async Task<(List<NotificationDto> Notifications, int TotalCount)> GetNotificationsAsync(
            Guid userId, int page = 1, int pageSize = 20, int? type = null, int? isRead = null)
        {
            var query = BuildNotificationQuery(userId, type, isRead);
            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.SentTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => MapToNotificationDto(n, false))
                .ToListAsync();

            return (notifications, totalCount);
        }

        /// <summary>
        /// 發送系統通知
        /// </summary>
        public async Task<bool> SendSystemNotificationAsync(Guid receiverId, string title, string content, int type = 0)
        {
            var receiver = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == receiverId);
            if (receiver == null) return false;

            _context.Notifications.Add(new Notification
            {
                NotifyId = Guid.NewGuid(),
                GetId = receiver.PersonId,
                SendId = receiver.PersonId,
                Type = type,
                IsRead = 0,
                SentTime = DateTime.Now,
                Receiver = receiver,
                Sender = receiver
            });

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 發送使用者通知
        /// </summary>
        public async Task<bool> SendUserNotificationAsync(Guid senderId, Guid receiverId, int type, Guid? relatedId = null)
        {
            var sender = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == senderId);
            var receiver = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == receiverId);

            if (sender == null || receiver == null || senderId == receiverId) return false;

            var existingNotification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.SendId == sender.PersonId &&
                                       n.GetId == receiver.PersonId &&
                                       n.Type == type);

            if (existingNotification != null)
            {
                existingNotification.SentTime = DateTime.Now;
                existingNotification.IsRead = 0;
                existingNotification.IsReadTime = null;
            }
            else
            {
                _context.Notifications.Add(new Notification
                {
                    NotifyId = Guid.NewGuid(),
                    GetId = receiver.PersonId,
                    SendId = sender.PersonId,
                    Type = type,
                    IsRead = 0,
                    SentTime = DateTime.Now,
                    Receiver = receiver,
                    Sender = sender
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 標記通知為已讀
        /// </summary>
        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            return await UpdateNotificationReadStatus(notificationId, userId, 1);
        }

        /// <summary>
        /// 標記通知為未讀
        /// </summary>
        public async Task<bool> MarkAsUnreadAsync(Guid notificationId, Guid userId)
        {
            return await UpdateNotificationReadStatus(notificationId, userId, 0);
        }

        /// <summary>
        /// 批量標記通知為已讀
        /// </summary>
        public async Task<bool> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId)
        {
            var notifications = await GetValidNotifications(notificationIds, userId);
            if (notifications.Count == 0) return true;

            foreach (var notification in notifications.Where(n => n.IsRead == 0))
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var userPerson = await GetPersonByUserId(userId);
            if (userPerson == null) return false;

            var unreadNotifications = await _context.Notifications
                .Where(n => n.GetId == userPerson.PersonId && n.IsRead == 0)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 刪除通知
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotifyId == notificationId);

            if (notification?.Receiver.UserId != userId) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 批量刪除通知
        /// </summary>
        public async Task<bool> DeleteMultipleNotificationsAsync(List<Guid> notificationIds, Guid userId)
        {
            var notifications = await GetValidNotifications(notificationIds, userId);
            if (notifications.Count == 0) return true;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 獲取未讀通知數量
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var userPerson = await GetPersonByUserId(userId);
            return userPerson == null ? 0 : await _context.Notifications
                .AsNoTracking() // 只讀統計
                .Where(n => n.GetId == userPerson.PersonId && n.IsRead == 0)
                .CountAsync();
        }

        /// <summary>
        /// 獲取通知統計資料
        /// </summary>
        public async Task<Dictionary<string, int>> GetNotificationStatsAsync(Guid userId)
        {
            var userPerson = await GetPersonByUserId(userId);
            if (userPerson == null) return [];

            var notifications = await _context.Notifications
                .AsNoTracking() // 只讀統計
                .Where(n => n.GetId == userPerson.PersonId)
                .ToListAsync();

            return new Dictionary<string, int>
            {
                ["Total"] = notifications.Count,
                ["Unread"] = notifications.Count(n => n.IsRead == 0),
                ["Read"] = notifications.Count(n => n.IsRead == 1),
                ["System"] = notifications.Count(n => n.Type == 0),
                ["Like"] = notifications.Count(n => n.Type == 1),
                ["Reply"] = notifications.Count(n => n.Type == 2),
                ["Follow"] = notifications.Count(n => n.Type == 3),
                ["Friend"] = notifications.Count(n => n.Type == 4),
                ["Other"] = notifications.Count(n => n.Type == 5)
            };
        }

        /// <summary>
        /// 清理過期通知
        /// </summary>
        public async Task<int> CleanupOldNotificationsAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var oldNotifications = await _context.Notifications
                .Where(n => n.IsRead == 1 && n.IsReadTime < cutoffDate)
                .ToListAsync();

            if (oldNotifications.Count == 0) return 0;

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
            return oldNotifications.Count;
        }

        // 私有輔助方法
        private IQueryable<Notification> BuildNotificationQuery(Guid userId, int? type, int? isRead)
        {
            var query = _context.Notifications
                .AsNoTracking() // 只讀查詢
                .Include(n => n.Receiver)
                .Include(n => n.Sender)
                .Where(n => n.GetId == userId);

            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            return query;
        }

        private async Task<Person?> GetPersonByUserId(Guid userId)
        {
            return await _context.Persons.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        private async Task<List<Notification>> GetValidNotifications(List<Guid> notificationIds, Guid userId)
        {
            return await _context.Notifications
                .Include(n => n.Receiver)
                .Where(n => notificationIds.Contains(n.NotifyId) && n.Receiver.UserId == userId)
                .ToListAsync();
        }

        private async Task<bool> UpdateNotificationReadStatus(Guid notificationId, Guid userId, int readStatus)
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotifyId == notificationId);

            if (notification?.Receiver.UserId != userId || notification.IsRead == readStatus)
                return notification?.Receiver.UserId == userId;

            notification.IsRead = readStatus;
            notification.IsReadTime = readStatus == 1 ? DateTime.Now : null;
            await _context.SaveChangesAsync();
            return true;
        }

        private static PersonDto? MapToPersonDto(Person? person, bool includeDetails = false)
        {
            if (person == null) return null;

            var dto = new PersonDto
            {
                PersonId = person.PersonId,
                UserId = person.UserId,
                DisplayName = person.DisplayName,
                AvatarPath = person.AvatarPath,
                IsPrivate = person.IsPrivate
            };

            if (includeDetails)
            {
                dto.Bio = person.Bio;
                dto.BannerPath = person.BannerPath;
                dto.WalletAddress = person.WalletAddress;
                dto.ModifyTime = person.ModifyTime;
            }

            return dto;
        }

        private static NotificationDto MapToNotificationDto(Notification n, bool includeDetails = false)
        {
            return new NotificationDto
            {
                NotificationId = n.NotifyId,
                UserId = n.GetId,
                SenderId = n.SendId,
                Type = n.Type,
                Title = GetNotificationTitle(n),
                Content = GetNotificationContent(n),
                Status = n.IsRead,
                CreateTime = n.SentTime,
                ReadTime = n.IsReadTime,
                User = MapToPersonDto(n.Receiver, includeDetails),
                Sender = MapToPersonDto(n.Sender, includeDetails)
            };
        }

        /// <summary>
        /// 獲取通知標題
        /// </summary>
        private static string GetNotificationTitle(Notification notification)
        {
            return notification.Type switch
            {
                0 => "系統通知",
                1 => "有人對您的內容按讚",
                2 => "有人回覆了您的內容",
                3 => "有人追蹤了您",
                4 => "收到好友請求",
                5 => "其他通知",
                _ => "未知通知"
            };
        }

        /// <summary>
        /// 獲取通知內容
        /// </summary>
        private static string GetNotificationContent(Notification notification)
        {
            var senderName = notification.Sender?.DisplayName ?? "系統";

            return notification.Type switch
            {
                0 => "系統通知內容",
                1 => $"{senderName} 對您的內容按讚",
                2 => $"{senderName} 回覆了您的內容",
                3 => $"{senderName} 開始追蹤您",
                4 => $"{senderName} 向您發送了好友請求",
                5 => $"{senderName} 發送了其他通知",
                _ => "未知通知內容"
            };
        }
    }
}