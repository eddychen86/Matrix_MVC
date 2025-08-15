namespace Matrix.Services
{
    /// <summary>
    /// 舉報服務
    /// </summary>
#pragma warning disable CS9113 // Parameter is unread
    public enum ReportType { User = 0, Article = 1 }
    public enum ReportStatus { Pending = 0, Processed = 1, Rejected = 2 }
    public class ReportService(ApplicationDbContext _context) : IReportService
#pragma warning disable CS9113
    {

        public async Task<bool> CreateReportAsync(Guid reporterId, Guid reportedUserId, Guid? articleId, string reason, string? description = null)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                TargetId = articleId ?? reportedUserId,
                Type = articleId.HasValue ? (int)ReportType.Article : (int)ReportType.User,
                Reason = string.IsNullOrWhiteSpace(description) ? reason : $"{reason} | {description}",
                Status = (int)ReportStatus.Pending
            };
            _context.Reports.Add(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<Report?> GetReportAsync(Guid id)
        {
            return _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Resolver)
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }

        //public async Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
        //    int page = 1,
        //    int pageSize = 20,
        //    int? status = null,
        //    Guid? reporterId = null,
        //    Guid? reportedUserId = null)
        //{
        //    var q = _context.Reports
        //        .Include(r => r.Reporter)
        //        .AsQueryable();

        //    if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        //    if (reporterId.HasValue) q = q.Where(r => r.ReporterId == reporterId.Value);
        //    if (reportedUserId.HasValue)
        //        q = q.Where(r => r.Type == (int)ReportType.User && r.TargetId == reportedUserId.Value);

        //    var total = await q.CountAsync();

        //    var list = await q
        //        .OrderByDescending(r => r.ReportId)   // 時間序 GUID 當作時間排序
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .AsNoTracking()
        //        .ToListAsync();

        //    return (list, total);
        //}

        public async Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
            int page = 1,
            int pageSize = 20,
            int? status = null,
            Guid? reporterId = null,
            Guid? reportedUserId = null)
        {
            // ✅ 舊簽名直接導到新簽名（其餘條件用 null）
            return await GetReportsAsync(
                page: page,
                pageSize: pageSize,
                status: status,
                type: null,
                keyword: null,
                from: null,
                to: null,
                reporterId: reporterId,
                reportedUserId: reportedUserId
            );
        }

        // ✅ 新簽名（包含 type/keyword/from/to）
        public async Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
            int page,
            int pageSize,
            int? status,
            int? type,
            string? keyword,
            DateTime? from,
            DateTime? to,
            Guid? reporterId = null,
            Guid? reportedUserId = null)
        {
            var q = _context.Reports
                .Include(r => r.Reporter)
                .AsQueryable();

            // 狀態
            if (status.HasValue)
                q = q.Where(r => r.Status == status.Value);

            // 類型（0=User, 1=Article）
            if (type.HasValue)
                q = q.Where(r => r.Type == type.Value);

            // 指定檢舉人
            if (reporterId.HasValue)
                q = q.Where(r => r.ReporterId == reporterId.Value);

            // 指定被檢舉使用者（僅在 Type=User 時）
            if (reportedUserId.HasValue)
                q = q.Where(r => r.Type == (int)ReportType.User && r.TargetId == reportedUserId.Value);

            // 關鍵字（Reason / Reporter.DisplayName / 若為 Article 也比對文章內容）
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();

                // 一次取出命中的文章 Id（避免 N+1）
                var articleIdsMatched = await _context.Articles
                    .Where(a => a.Content.Contains(k))
                    .Select(a => a.ArticleId)
                    .ToListAsync();

                q = q.Where(r =>
                    r.Reason.Contains(k) ||
                    (r.Reporter != null && r.Reporter.DisplayName.Contains(k)) ||
                    (r.Type == (int)ReportType.Article && articleIdsMatched.Contains(r.TargetId))
                );
            }

            // 日期：用 ProcessTime 當 ModifyTime 篩選（僅對已處理/已駁回會生效）
            if (from.HasValue)
                q = q.Where(r => r.ProcessTime.HasValue && r.ProcessTime.Value >= from.Value);

            if (to.HasValue)
            {
                var toEnd = to.Value.Date.AddDays(1); // 含當日 23:59:59
                q = q.Where(r => r.ProcessTime.HasValue && r.ProcessTime.Value < toEnd);
            }

            var total = await q.CountAsync();

            q = q.OrderByDescending(r => r.ProcessTime) // null(未處理) 會自然排後面
                 .ThenByDescending(r => r.ReportId);

            var list = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (list, total);
        }

        public async Task<bool> ProcessReportAsync(Guid reportId, Guid adminId, string? adminNote = null)
        {
            var r = await _context.Reports.FirstOrDefaultAsync(x => x.ReportId == reportId);
            if (r is null) return false;

            r.Status = (int)ReportStatus.Processed;
            r.ResolverId = adminId;
            r.ProcessTime = DateTime.UtcNow;
            // 若要存 adminNote 需新增欄位，這裡先略
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectReportAsync(Guid reportId, Guid adminId, string? adminNote = null)
        {
            var r = await _context.Reports.FirstOrDefaultAsync(x => x.ReportId == reportId);
            if (r is null) return false;

            r.Status = (int)ReportStatus.Rejected;
            r.ResolverId = adminId;
            r.ProcessTime = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        // 這是 IStatusManageable<Guid> 要求實作的方法
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            var r = await _context.Reports.FirstOrDefaultAsync(x => x.ReportId == id);
            if (r is null) return false;

            r.Status = status;
            // 沒有 Report.ModifyTime 欄位就不寫；ProcessTime 僅在處理/駁回時更新
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Dictionary<string, int>> GetReportStatsAsync()
        {
            var data = await _context.Reports
                .GroupBy(r => r.Status)
                .Select(g => new { g.Key, Cnt = g.Count() })
                .ToListAsync();

            return data.ToDictionary(
                x => x.Key switch { 0 => "Pending", 1 => "Processed", 2 => "Rejected", _ => x.Key.ToString() },
                x => x.Cnt);
        }

        public async Task<List<Report>> GetReportsByUserAsync(Guid userId, int limit = 10)
        {
            return await _context.Reports
                .Where(r => r.ReporterId == userId || (r.Type == (int)ReportType.User && r.TargetId == userId))
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.ReportId)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}