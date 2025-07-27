namespace Matrix.Services
{
    /// <summary>
    /// 舉報服務
    /// </summary>
    #pragma warning disable CS9113 // Parameter is unread
    public class ReportService(ApplicationDbContext _context) : IReportService
    #pragma warning disable CS9113
    {

        public Task<bool> CreateReportAsync(Guid reporterId, Guid reportedUserId, Guid? articleId, string reason, string? description = null)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<Report?> GetReportAsync(Guid id)
        {
            return Task.FromException<Report?>(new NotImplementedException());
        }

        public Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
            int page = 1,
            int pageSize = 20,
            int? status = null,
            Guid? reporterId = null,
            Guid? reportedUserId = null)
        {
            return Task.FromException<(List<Report>, int)>(new NotImplementedException());
        }

        public Task<bool> ProcessReportAsync(Guid reportId, Guid adminId, string? adminNote = null)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> RejectReportAsync(Guid reportId, Guid adminId, string? adminNote = null)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<Dictionary<string, int>> GetReportStatsAsync()
        {
            return Task.FromException<Dictionary<string, int>>(new NotImplementedException());
        }

        public Task<List<Report>> GetReportsByUserAsync(Guid userId, int limit = 10)
        {
            return Task.FromException<List<Report>>(new NotImplementedException());
        }
    }
}