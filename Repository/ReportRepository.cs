using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 舉報資料存取實作
    /// </summary>
    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Report>> GetPendingReportsAsync(int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.Resolver)
                .Where(r => r.Status == 0) // 假設 Status 0 表示待處理
                .OrderBy(r => r.ProcessTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetArticleReportsAsync(Guid articleId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.Resolver)
                .Where(r => r.TargetId == articleId)
                .OrderByDescending(r => r.ProcessTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetUserReportsAsync(Guid reporterId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Resolver)
                .Where(r => r.ReporterId == reporterId)
                .OrderByDescending(r => r.ProcessTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportedUserReportsAsync(Guid reportedUserId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Reporter)
                .Include(r => r.Resolver)
                .Where(r => r.TargetId == reportedUserId)
                .OrderByDescending(r => r.ProcessTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> HasUserReportedArticleAsync(Guid reporterId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(r => r.ReporterId == reporterId && r.TargetId == articleId);
        }

        public async Task<int> CountPendingReportsAsync()
        {
            return await _dbSet.CountAsync(r => r.Status == 0);
        }

        public async Task<int> CountArticleReportsAsync(Guid articleId)
        {
            return await _dbSet.CountAsync(r => r.TargetId == articleId);
        }

        public async Task UpdateReportStatusAsync(Guid reportId, int status, Guid? handledById = null, string? handlerNote = null)
        {
            var report = await _dbSet.FindAsync(reportId);
            if (report != null)
            {
                report.Status = status;
                report.ResolverId = handledById;
                // report.HandlerNote = handlerNote; // Model 中無此欄位
                report.ProcessTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<int, int>> GetReportStatsByTypeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.ProcessTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.ProcessTime <= endDate.Value);

            return await query
                .GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        public async Task BatchUpdateReportStatusAsync(IEnumerable<Guid> reportIds, int status, Guid handledById, string? handlerNote = null)
        {
            var reports = await _dbSet
                .Where(r => reportIds.Contains(r.ReportId))
                .ToListAsync();

            foreach (var report in reports)
            {
                report.Status = status;
                report.ResolverId = handledById;
                report.ProcessTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
