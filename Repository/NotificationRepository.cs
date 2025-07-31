using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 通知資料存取實作
    /// </summary>
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByReceiverIdAsync(Guid receiverId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(n => n.GetId == receiverId)
                .OrderByDescending(n => n.SentTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid receiverId)
        {
            return await _dbSet
                .Where(n => n.GetId == receiverId && n.IsRead == 0)
                .OrderByDescending(n => n.SentTime)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkMultipleAsReadAsync(IEnumerable<Guid> notificationIds)
        {
            var notifications = await _dbSet
                .Where(n => notificationIds.Contains(n.NotifyId))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid receiverId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.GetId == receiverId && n.IsRead == 0)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountUnreadNotificationsAsync(Guid receiverId)
        {
            return await _dbSet
                .CountAsync(n => n.GetId == receiverId && n.IsRead == 0);
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(Guid receiverId, int type, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(n => n.GetId == receiverId && n.Type == type)
                .OrderByDescending(n => n.SentTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task DeleteExpiredNotificationsAsync(DateTime expiredBefore)
        {
            var expiredNotifications = await _dbSet
                .Where(n => n.SentTime < expiredBefore)
                .ToListAsync();

            _dbSet.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync();
        }
    }
}
