using System;
using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;

namespace Matrix.Services;

/// <summary>
/// 通知服務類別
/// 
/// 此服務負責處理與通知相關的業務邏輯，包括：
/// - 通知的 CRUD 操作
/// - 發送各類型通知（系統通知、讚通知、回覆通知等）
/// - 標記通知為已讀/未讀
/// - 通知的搜尋和篩選
/// - 通知的統計和管理
/// 
/// 注意事項：
/// - 所有方法都應該包含適當的錯誤處理
/// - 包含完整的資料驗證邏輯
/// - 支援非同步操作以提高效能
/// - 考慮通知的優先級和時效性
/// </summary>
public class NotificationService
{
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// 建構函式
    /// 用途：初始化服務並注入資料庫上下文
    /// </summary>
    /// <param name="context">資料庫上下文</param>
    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 根據 ID 獲取通知資料
    /// 用途：查詢特定通知的完整資料
    /// </summary>
    /// <param name="id">通知 ID</param>
    /// <returns>通知資料傳輸物件，如果不存在則返回 null</returns>
    public async Task<NotificationDto?> GetNotificationAsync(Guid id)
    {
        try
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(n => n.NotifyId == id);
            
            if (notification == null) return null;
            
            return new NotificationDto
            {
                NotificationId = notification.NotifyId,
                UserId = notification.GetId,
                SenderId = notification.SendId,
                Type = notification.Type,
                Title = GetNotificationTitle(notification),
                Content = GetNotificationContent(notification),
                Status = notification.IsRead,
                CreateTime = notification.SentTime,
                ReadTime = notification.IsReadTime,
                RelatedId = null, // 需要根據通知類型設定
                User = notification.Receiver != null ? new PersonDto
                {
                    PersonId = notification.Receiver.PersonId,
                    UserId = notification.Receiver.UserId,
                    DisplayName = notification.Receiver.DisplayName,
                    Bio = notification.Receiver.Bio,
                    AvatarPath = notification.Receiver.AvatarPath,
                    BannerPath = notification.Receiver.BannerPath,
                    ExternalUrl = notification.Receiver.ExternalUrl,
                    IsPrivate = notification.Receiver.IsPrivate,
                    WalletAddress = notification.Receiver.WalletAddress,
                    ModifyTime = notification.Receiver.ModifyTime
                } : null,
                Sender = notification.Sender != null ? new PersonDto
                {
                    PersonId = notification.Sender.PersonId,
                    UserId = notification.Sender.UserId,
                    DisplayName = notification.Sender.DisplayName,
                    Bio = notification.Sender.Bio,
                    AvatarPath = notification.Sender.AvatarPath,
                    BannerPath = notification.Sender.BannerPath,
                    ExternalUrl = notification.Sender.ExternalUrl,
                    IsPrivate = notification.Sender.IsPrivate,
                    WalletAddress = notification.Sender.WalletAddress,
                    ModifyTime = notification.Sender.ModifyTime
                } : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取通知資料時發生錯誤: {ex.Message}");
            throw new Exception("獲取通知資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取使用者的通知列表
    /// 用途：分頁查詢使用者的通知列表
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁數量</param>
    /// <param name="type">通知類型篩選</param>
    /// <param name="isRead">已讀狀態篩選</param>
    /// <returns>通知列表和總數</returns>
    public async Task<(List<NotificationDto> Notifications, int TotalCount)> GetNotificationsAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        int? type = null, 
        int? isRead = null)
    {
        try
        {
            var query = _context.Notifications
                .Include(n => n.Receiver)
                .Include(n => n.Sender)
                .Where(n => n.GetId == userId);
            
            // 如果指定了通知類型，進行篩選
            if (type.HasValue)
            {
                query = query.Where(n => n.Type == type.Value);
            }
            
            // 如果指定了已讀狀態，進行篩選
            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }
            
            var totalCount = await query.CountAsync();
            
            var notifications = await query
                .OrderByDescending(n => n.SentTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
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
                    RelatedId = null, // 需要根據通知類型設定
                    User = n.Receiver != null ? new PersonDto
                    {
                        PersonId = n.Receiver.PersonId,
                        UserId = n.Receiver.UserId,
                        DisplayName = n.Receiver.DisplayName,
                        AvatarPath = n.Receiver.AvatarPath,
                        IsPrivate = n.Receiver.IsPrivate
                    } : null,
                    Sender = n.Sender != null ? new PersonDto
                    {
                        PersonId = n.Sender.PersonId,
                        UserId = n.Sender.UserId,
                        DisplayName = n.Sender.DisplayName,
                        AvatarPath = n.Sender.AvatarPath,
                        IsPrivate = n.Sender.IsPrivate
                    } : null
                })
                .ToListAsync();
            
            return (notifications, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取通知列表時發生錯誤: {ex.Message}");
            throw new Exception("獲取通知列表失敗", ex);
        }
    }
    
    /// <summary>
    /// 發送系統通知
    /// 用途：發送系統級通知給指定使用者
    /// </summary>
    /// <param name="receiverId">接收者 ID</param>
    /// <param name="title">通知標題</param>
    /// <param name="content">通知內容</param>
    /// <param name="type">通知類型</param>
    /// <returns>是否發送成功</returns>
    public async Task<bool> SendSystemNotificationAsync(Guid receiverId, string title, string content, int type = 0)
    {
        try
        {
            // 檢查接收者是否存在
            var receiver = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == receiverId);
            if (receiver == null)
            {
                throw new InvalidOperationException("找不到指定的接收者");
            }
            
            // 建立通知實體
            var notification = new Notification
            {
                NotifyId = Guid.NewGuid(),
                GetId = receiver.PersonId,
                SendId = receiver.PersonId, // 系統通知發送者設為接收者本身
                Type = type,
                IsRead = 0, // 未讀
                SentTime = DateTime.Now,
                IsReadTime = null,
                Receiver = receiver,
                Sender = receiver
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"發送系統通知時發生錯誤: {ex.Message}");
            throw new Exception("發送系統通知失敗", ex);
        }
    }
    
    /// <summary>
    /// 發送使用者通知
    /// 用途：使用者之間的通知（如讚、回覆、追蹤等）
    /// </summary>
    /// <param name="senderId">發送者 ID</param>
    /// <param name="receiverId">接收者 ID</param>
    /// <param name="type">通知類型</param>
    /// <param name="relatedId">相關資料 ID</param>
    /// <returns>是否發送成功</returns>
    public async Task<bool> SendUserNotificationAsync(Guid senderId, Guid receiverId, int type, Guid? relatedId = null)
    {
        try
        {
            // 檢查發送者是否存在
            var sender = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == senderId);
            if (sender == null)
            {
                throw new InvalidOperationException("找不到指定的發送者");
            }
            
            // 檢查接收者是否存在
            var receiver = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == receiverId);
            if (receiver == null)
            {
                throw new InvalidOperationException("找不到指定的接收者");
            }
            
            // 避免自己給自己發送通知
            if (senderId == receiverId)
            {
                return false;
            }
            
            // 檢查是否已有相同的通知（避免重複通知）
            var existingNotification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.SendId == sender.PersonId && 
                                       n.GetId == receiver.PersonId && 
                                       n.Type == type);
            
            if (existingNotification != null)
            {
                // 更新現有通知的時間
                existingNotification.SentTime = DateTime.Now;
                existingNotification.IsRead = 0; // 重設為未讀
                existingNotification.IsReadTime = null;
                await _context.SaveChangesAsync();
                return true;
            }
            
            // 建立新通知實體
            var notification = new Notification
            {
                NotifyId = Guid.NewGuid(),
                GetId = receiver.PersonId,
                SendId = sender.PersonId,
                Type = type,
                IsRead = 0, // 未讀
                SentTime = DateTime.Now,
                IsReadTime = null,
                Receiver = receiver,
                Sender = sender
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"發送使用者通知時發生錯誤: {ex.Message}");
            throw new Exception("發送使用者通知失敗", ex);
        }
    }
    
    /// <summary>
    /// 標記通知為已讀
    /// 用途：將指定的通知標記為已讀狀態
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID（用於權限檢查）</param>
    /// <returns>是否標記成功</returns>
    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotifyId == notificationId);
            
            if (notification == null)
            {
                throw new InvalidOperationException("找不到指定的通知");
            }
            
            // 檢查權限：只有接收者可以標記為已讀
            if (notification.Receiver.UserId != userId)
            {
                throw new UnauthorizedAccessException("沒有權限標記此通知");
            }
            
            // 如果已經是已讀狀態，直接返回成功
            if (notification.IsRead == 1)
            {
                return true;
            }
            
            notification.IsRead = 1;
            notification.IsReadTime = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"標記通知為已讀時發生錯誤: {ex.Message}");
            throw new Exception("標記通知為已讀失敗", ex);
        }
    }
    
    /// <summary>
    /// 標記通知為未讀
    /// 用途：將指定的通知標記為未讀狀態
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID（用於權限檢查）</param>
    /// <returns>是否標記成功</returns>
    public async Task<bool> MarkAsUnreadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotifyId == notificationId);
            
            if (notification == null)
            {
                throw new InvalidOperationException("找不到指定的通知");
            }
            
            // 檢查權限：只有接收者可以標記為未讀
            if (notification.Receiver.UserId != userId)
            {
                throw new UnauthorizedAccessException("沒有權限標記此通知");
            }
            
            // 如果已經是未讀狀態，直接返回成功
            if (notification.IsRead == 0)
            {
                return true;
            }
            
            notification.IsRead = 0;
            notification.IsReadTime = null;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"標記通知為未讀時發生錯誤: {ex.Message}");
            throw new Exception("標記通知為未讀失敗", ex);
        }
    }
    
    /// <summary>
    /// 批量標記通知為已讀
    /// 用途：將多個通知標記為已讀狀態
    /// </summary>
    /// <param name="notificationIds">通知 ID 列表</param>
    /// <param name="userId">使用者 ID（用於權限檢查）</param>
    /// <returns>是否標記成功</returns>
    public async Task<bool> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Include(n => n.Receiver)
                .Where(n => notificationIds.Contains(n.NotifyId))
                .ToListAsync();
            
            if (notifications.Count == 0)
            {
                return true; // 沒有找到通知，視為成功
            }
            
            // 檢查權限：只有接收者可以標記為已讀
            var unauthorizedNotifications = notifications
                .Where(n => n.Receiver.UserId != userId)
                .ToList();
            
            if (unauthorizedNotifications.Any())
            {
                throw new UnauthorizedAccessException("沒有權限標記某些通知");
            }
            
            // 標記為已讀
            foreach (var notification in notifications.Where(n => n.IsRead == 0))
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"批量標記通知為已讀時發生錯誤: {ex.Message}");
            throw new Exception("批量標記通知為已讀失敗", ex);
        }
    }
    
    /// <summary>
    /// 標記所有通知為已讀
    /// 用途：將使用者的所有通知標記為已讀狀態
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>是否標記成功</returns>
    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var userPerson = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPerson == null)
            {
                throw new InvalidOperationException("找不到指定的使用者");
            }
            
            var unreadNotifications = await _context.Notifications
                .Where(n => n.GetId == userPerson.PersonId && n.IsRead == 0)
                .ToListAsync();
            
            if (unreadNotifications.Count == 0)
            {
                return true; // 沒有未讀通知，視為成功
            }
            
            // 標記為已讀
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = 1;
                notification.IsReadTime = DateTime.Now;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"標記所有通知為已讀時發生錯誤: {ex.Message}");
            throw new Exception("標記所有通知為已讀失敗", ex);
        }
    }
    
    /// <summary>
    /// 刪除通知
    /// 用途：刪除指定的通知
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <param name="userId">使用者 ID（用於權限檢查）</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotifyId == notificationId);
            
            if (notification == null)
            {
                throw new InvalidOperationException("找不到指定的通知");
            }
            
            // 檢查權限：只有接收者可以刪除通知
            if (notification.Receiver.UserId != userId)
            {
                throw new UnauthorizedAccessException("沒有權限刪除此通知");
            }
            
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"刪除通知時發生錯誤: {ex.Message}");
            throw new Exception("刪除通知失敗", ex);
        }
    }
    
    /// <summary>
    /// 批量刪除通知
    /// 用途：刪除多個通知
    /// </summary>
    /// <param name="notificationIds">通知 ID 列表</param>
    /// <param name="userId">使用者 ID（用於權限檢查）</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteMultipleNotificationsAsync(List<Guid> notificationIds, Guid userId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Include(n => n.Receiver)
                .Where(n => notificationIds.Contains(n.NotifyId))
                .ToListAsync();
            
            if (notifications.Count == 0)
            {
                return true; // 沒有找到通知，視為成功
            }
            
            // 檢查權限：只有接收者可以刪除通知
            var unauthorizedNotifications = notifications
                .Where(n => n.Receiver.UserId != userId)
                .ToList();
            
            if (unauthorizedNotifications.Any())
            {
                throw new UnauthorizedAccessException("沒有權限刪除某些通知");
            }
            
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"批量刪除通知時發生錯誤: {ex.Message}");
            throw new Exception("批量刪除通知失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取未讀通知數量
    /// 用途：獲取使用者未讀通知的數量
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>未讀通知數量</returns>
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var userPerson = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPerson == null)
            {
                return 0;
            }
            
            return await _context.Notifications
                .Where(n => n.GetId == userPerson.PersonId && n.IsRead == 0)
                .CountAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取未讀通知數量時發生錯誤: {ex.Message}");
            throw new Exception("獲取未讀通知數量失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取通知統計資料
    /// 用途：獲取使用者的通知統計資料
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>通知統計資料</returns>
    public async Task<Dictionary<string, int>> GetNotificationStatsAsync(Guid userId)
    {
        try
        {
            var userPerson = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPerson == null)
            {
                return new Dictionary<string, int>();
            }
            
            var notifications = await _context.Notifications
                .Where(n => n.GetId == userPerson.PersonId)
                .ToListAsync();
            
            var stats = new Dictionary<string, int>
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
            
            return stats;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取通知統計資料時發生錯誤: {ex.Message}");
            throw new Exception("獲取通知統計資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 清理過期通知
    /// 用途：清理指定天數前的已讀通知
    /// </summary>
    /// <param name="days">保留天數</param>
    /// <returns>清理的通知數量</returns>
    public async Task<int> CleanupOldNotificationsAsync(int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            
            var oldNotifications = await _context.Notifications
                .Where(n => n.IsRead == 1 && n.IsReadTime < cutoffDate)
                .ToListAsync();
            
            if (oldNotifications.Count == 0)
            {
                return 0;
            }
            
            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
            
            return oldNotifications.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理過期通知時發生錯誤: {ex.Message}");
            throw new Exception("清理過期通知失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取通知標題
    /// 用途：根據通知類型生成適當的標題
    /// </summary>
    /// <param name="notification">通知實體</param>
    /// <returns>通知標題</returns>
    private string GetNotificationTitle(Notification notification)
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
    /// 用途：根據通知類型生成適當的內容
    /// </summary>
    /// <param name="notification">通知實體</param>
    /// <returns>通知內容</returns>
    private string GetNotificationContent(Notification notification)
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
