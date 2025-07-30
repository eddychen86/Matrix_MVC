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
                .Where(n => n.ReceiverId == receiverId)
                .OrderByDescending(n => n.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid receiverId)
        {
            return await _dbSet
                .Where(n => n.ReceiverId == receiverId && !n.IsRead)
                .OrderByDescending(n => n.CreateTime)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkMultipleAsReadAsync(IEnumerable<Guid> notificationIds)
        {
            var notifications = await _dbSet
                .Where(n => notificationIds.Contains(n.NotificationId))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid receiverId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.ReceiverId == receiverId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountUnreadNotificationsAsync(Guid receiverId)
        {
            return await _dbSet
                .CountAsync(n => n.ReceiverId == receiverId && !n.IsRead);
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(Guid receiverId, int type, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(n => n.ReceiverId == receiverId && n.Type == type)
                .OrderByDescending(n => n.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task DeleteExpiredNotificationsAsync(DateTime expiredBefore)
        {
            var expiredNotifications = await _dbSet
                .Where(n => n.CreateTime < expiredBefore)
                .ToListAsync();

            _dbSet.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync();
        }
    }
}