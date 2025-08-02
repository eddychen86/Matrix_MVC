using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 舉報資料存取介面
    /// </summary>
    public interface IReportRepository : IRepository<Report>
    {
        /// <summary>取得待處理的舉報</summary>
        Task<IEnumerable<Report>> GetPendingReportsAsync(int page = 1, int pageSize = 20);

        /// <summary>取得特定文章的舉報</summary>
        Task<IEnumerable<Report>> GetArticleReportsAsync(Guid articleId, int page = 1, int pageSize = 20);

        /// <summary>取得特定用戶的舉報記錄</summary>
        Task<IEnumerable<Report>> GetUserReportsAsync(Guid reporterId, int page = 1, int pageSize = 20);

        /// <summary>取得被舉報用戶的舉報記錄</summary>
        Task<IEnumerable<Report>> GetReportedUserReportsAsync(Guid reportedUserId, int page = 1, int pageSize = 20);

        /// <summary>檢查用戶是否已舉報過特定文章</summary>
        Task<bool> HasUserReportedArticleAsync(Guid reporterId, Guid articleId);

        /// <summary>計算待處理舉報數量</summary>
        Task<int> CountPendingReportsAsync();

        /// <summary>計算特定文章的舉報數量</summary>
        Task<int> CountArticleReportsAsync(Guid articleId);

        /// <summary>更新舉報處理狀態</summary>
        Task UpdateReportStatusAsync(Guid reportId, int status, Guid? handledById = null, string? handlerNote = null);

        /// <summary>取得依類型分組的舉報統計</summary>
        Task<Dictionary<int, int>> GetReportStatsByTypeAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>批量更新舉報狀態</summary>
        Task BatchUpdateReportStatusAsync(IEnumerable<Guid> reportIds, int status, Guid handledById, string? handlerNote = null);
    }
}