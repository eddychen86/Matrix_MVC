using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 管理員活動記錄資料存取實作（原登入記錄擴展）
    /// </summary>
    public class LoginRecordRepository : BaseRepository<AdminActivityLog>, ILoginRecordRepository
    {
        public LoginRecordRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<AdminActivityLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminActivityLog>> GetSuccessfulLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(lr => lr.UserId == userId && lr.IsSuccessful)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminActivityLog>> GetFailedLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(lr => lr.UserId == userId && !lr.IsSuccessful)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminActivityLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.ActionTime >= startDate && lr.ActionTime <= endDate)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminActivityLog>> GetByIpAddressAsync(string ipAddress, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.IpAddress == ipAddress)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountFailedLoginsAsync(Guid userId, DateTime since)
        {
            return await _dbSet
                .Where(lr => lr.UserId == userId && lr.ActionTime >= since && !lr.IsSuccessful)
                .CountAsync();
        }

        public async Task<int> CountFailedLoginsByIpAsync(string ipAddress, DateTime since)
        {
            return await _dbSet
                .Where(lr => lr.IpAddress == ipAddress && lr.ActionTime >= since && !lr.IsSuccessful)
                .CountAsync();
        }

        public async Task<AdminActivityLog?> GetLastSuccessfulLoginAsync(Guid userId)
        {
            return await _dbSet
                .Where(lr => lr.UserId == userId && lr.IsSuccessful && lr.ActionType == "LOGIN")
                .OrderByDescending(lr => lr.ActionTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AdminActivityLog>> GetAnomalousLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // 取得用戶常用的IP地址
            var commonIps = await _dbSet
                .Where(lr => lr.UserId == userId)
                .GroupBy(lr => lr.IpAddress)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            // 找出不在常用IP列表中的活動記錄
            return await _dbSet
                .Where(lr => lr.UserId == userId && !commonIps.Contains(lr.IpAddress!))
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task CleanupExpiredRecordsAsync(DateTime expiredBefore)
        {
            var expiredRecords = await _dbSet
                .Where(lr => lr.ActionTime < expiredBefore)
                .ToListAsync();

            _dbSet.RemoveRange(expiredRecords);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, int>> GetLoginStatsAsync(DateTime startDate, DateTime endDate)
        {
            var query = _dbSet.Where(lr => lr.ActionTime >= startDate && lr.ActionTime <= endDate);

            var totalActivities = await query.CountAsync();
            var successfulActivities = await query.Where(lr => lr.IsSuccessful).CountAsync();
            var failedActivities = await query.Where(lr => !lr.IsSuccessful).CountAsync();
            var uniqueUsers = await query.Select(lr => lr.UserId).Distinct().CountAsync();

            return new Dictionary<string, int>
            {
                ["TotalActivities"] = totalActivities,
                ["SuccessfulActivities"] = successfulActivities,
                ["FailedActivities"] = failedActivities,
                ["UniqueUsers"] = uniqueUsers
            };
        }

        /// <summary>
        /// 取得特定操作類型的活動記錄
        /// </summary>
        public async Task<IEnumerable<AdminActivityLog>> GetByActionTypeAsync(string actionType, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.ActionType == actionType)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 取得管理員在特定頁面的活動記錄
        /// </summary>
        public async Task<IEnumerable<AdminActivityLog>> GetByPagePathAsync(string pagePath, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.PagePath == pagePath)
                .OrderByDescending(lr => lr.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 記錄管理員活動
        /// </summary>
        public async Task LogActivityAsync(AdminActivityLog activity)
        {
            await _dbSet.AddAsync(activity);
            await _context.SaveChangesAsync();
        }
    }
}
