namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 舉報服務介面
    /// 定義舉報相關的業務邏輯操作
    /// </summary>
    public interface IReportService : IStatusManageable<Guid>
    {
        /// <summary>
        /// 建立新舉報
        /// </summary>
        /// <param name="reporterId">舉報者 ID</param>
        /// <param name="reportedUserId">被舉報使用者 ID</param>
        /// <param name="articleId">被舉報文章 ID（可選）</param>
        /// <param name="reason">舉報原因</param>
        /// <param name="description">舉報描述</param>
        /// <returns>建立是否成功</returns>
        Task<bool> CreateReportAsync(Guid reporterId, Guid reportedUserId, Guid? articleId, string reason, string? description = null);

        /// <summary>
        /// 根據 ID 獲取舉報資料
        /// </summary>
        /// <param name="id">舉報 ID</param>
        /// <returns>舉報資料，如果不存在則返回 null</returns>
        Task<Report?> GetReportAsync(Guid id);

        /// <summary>
        /// 分頁查詢舉報列表
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <param name="status">舉報狀態</param>
        /// <param name="reporterId">舉報者 ID</param>
        /// <param name="reportedUserId">被舉報使用者 ID</param>
        /// <returns>舉報列表和總數量</returns>
        Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
            int page = 1,
            int pageSize = 20,
            int? status = null,
            Guid? reporterId = null,
            Guid? reportedUserId = null);


        // ✅ 新增完整參數多載（給後台列表用）
        Task<(List<Report> Reports, int TotalCount)> GetReportsAsync(
            int page,
            int pageSize,
            int? status,
            int? type,
            string? keyword,
            DateTime? from,
            DateTime? to,
            Guid? reporterId = null,
            Guid? reportedUserId = null);

        /// <summary>
        /// 處理舉報（接受舉報）
        /// </summary>
        /// <param name="reportId">舉報 ID</param>
        /// <param name="adminId">管理員 ID</param>
        /// <param name="adminNote">管理員備註</param>
        /// <returns>處理是否成功</returns>
        Task<bool> ProcessReportAsync(Guid reportId, Guid adminId, string? adminNote = null);

        /// <summary>
        /// 拒絕舉報
        /// </summary>
        /// <param name="reportId">舉報 ID</param>
        /// <param name="adminId">管理員 ID</param>
        /// <param name="adminNote">管理員備註</param>
        /// <returns>拒絕是否成功</returns>
        Task<bool> RejectReportAsync(Guid reportId, Guid adminId, string? adminNote = null);

        /// <summary>
        /// 獲取舉報統計資料
        /// </summary>
        /// <returns>舉報統計資料</returns>
        Task<Dictionary<string, int>> GetReportStatsAsync();

        /// <summary>
        /// 獲取使用者相關的舉報記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="limit">限制數量</param>
        /// <returns>舉報記錄列表</returns>
        Task<List<Report>> GetReportsByUserAsync(Guid userId, int limit = 10);

        /// <summary>
        /// 獲取待處理的舉報數量
        /// </summary>
        /// <returns>待處理舉報數量</returns>
        Task<int> GetPendingReportsCountAsync();
    }
}