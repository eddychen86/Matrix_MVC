using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 登入記錄資料存取實作
    /// </summary>
    public class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
    {
        public LoginRecordRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LoginRecord>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginRecord>> GetSuccessfulLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // 注意: Model 中沒有 IsSuccess 欄位，此方法回傳所有登入記錄
            return await _dbSet
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginRecord>> GetFailedLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // 注意: Model 中沒有 IsSuccess 欄位，此方法回傳所有登入記錄
            return await _dbSet
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.LoginTime >= startDate && lr.LoginTime <= endDate)
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginRecord>> GetByIpAddressAsync(string ipAddress, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(lr => lr.User)
                .Where(lr => lr.IpAddress == ipAddress)
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> CountFailedLoginsAsync(Guid userId, DateTime since)
        {
            // 注意: Model 中沒有 IsSuccess 欄位，無法計數失敗登入
            return Task.FromResult(0);
        }

        public Task<int> CountFailedLoginsByIpAsync(string ipAddress, DateTime since)
        {
            // 注意: Model 中沒有 IsSuccess 欄位，無法計數失敗登入
            return Task.FromResult(0);
        }

        public async Task<LoginRecord?> GetLastSuccessfulLoginAsync(Guid userId)
        {
            // 注意: Model 中沒有 IsSuccess 欄位，此方法回傳最後一筆登入記錄
            return await _dbSet
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.LoginTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LoginRecord>> GetAnomalousLoginsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            // 取得用戶常用的IP地址
            var commonIps = await _dbSet
                .Where(lr => lr.UserId == userId)
                .GroupBy(lr => lr.IpAddress)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            // 找出不在常用IP列表中的登入記錄
            return await _dbSet
                .Where(lr => lr.UserId == userId && !commonIps.Contains(lr.IpAddress!))
                .OrderByDescending(lr => lr.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task CleanupExpiredRecordsAsync(DateTime expiredBefore)
        {
            var expiredRecords = await _dbSet
                .Where(lr => lr.LoginTime < expiredBefore)
                .ToListAsync();

            _dbSet.RemoveRange(expiredRecords);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, int>> GetLoginStatsAsync(DateTime startDate, DateTime endDate)
        {
            var query = _dbSet.Where(lr => lr.LoginTime >= startDate && lr.LoginTime <= endDate);

            var totalLogins = await query.CountAsync();
            var uniqueUsers = await query.Select(lr => lr.UserId).Distinct().CountAsync();

            return new Dictionary<string, int>
            {
                ["TotalLogins"] = totalLogins,
                ["SuccessfulLogins"] = 0, // 注意: Model 中沒有 IsSuccess 欄位
                ["FailedLogins"] = 0, // 注意: Model 中沒有 IsSuccess 欄位
                ["UniqueUsers"] = uniqueUsers
            };
        }
    }
}
