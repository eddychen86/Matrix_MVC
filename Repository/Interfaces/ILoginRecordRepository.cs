using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 管理員活動記錄資料存取介面（原登入記錄擴展）
    /// </summary>
    public interface ILoginRecordRepository : IRepository<AdminActivityLog>
    {
        /// <summary>取得用戶的活動記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得成功的操作記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetSuccessfulLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得失敗的操作記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetFailedLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得特定時間範圍的活動記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);

        /// <summary>取得特定IP的活動記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetByIpAddressAsync(string ipAddress, int page = 1, int pageSize = 20);

        /// <summary>計算用戶在特定時間內的失敗操作次數</summary>
        Task<int> CountFailedLoginsAsync(Guid userId, DateTime since);

        /// <summary>計算IP在特定時間內的失敗操作次數</summary>
        Task<int> CountFailedLoginsByIpAsync(string ipAddress, DateTime since);

        /// <summary>取得用戶最後一次成功操作記錄</summary>
        Task<AdminActivityLog?> GetLastSuccessfulLoginAsync(Guid userId);

        /// <summary>取得異常活動記錄（不同地點、設備等）</summary>
        Task<IEnumerable<AdminActivityLog>> GetAnomalousLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>清理過期活動記錄</summary>
        Task CleanupExpiredRecordsAsync(DateTime expiredBefore);

        /// <summary>取得活動統計</summary>
        Task<Dictionary<string, int>> GetLoginStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>取得特定操作類型的活動記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetByActionTypeAsync(string actionType, int page = 1, int pageSize = 20);

        /// <summary>取得管理員在特定頁面的活動記錄</summary>
        Task<IEnumerable<AdminActivityLog>> GetByPagePathAsync(string pagePath, int page = 1, int pageSize = 20);

        /// <summary>記錄管理員活動</summary>
        Task LogActivityAsync(AdminActivityLog activity);
    }
}