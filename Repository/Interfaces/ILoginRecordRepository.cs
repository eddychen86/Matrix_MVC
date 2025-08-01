using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 登入記錄資料存取介面
    /// </summary>
    public interface ILoginRecordRepository : IRepository<LoginRecord>
    {
        /// <summary>取得用戶的登入記錄</summary>
        Task<IEnumerable<LoginRecord>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得成功登入記錄</summary>
        Task<IEnumerable<LoginRecord>> GetSuccessfulLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得失敗登入記錄</summary>
        Task<IEnumerable<LoginRecord>> GetFailedLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得特定時間範圍的登入記錄</summary>
        Task<IEnumerable<LoginRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);

        /// <summary>取得特定IP的登入記錄</summary>
        Task<IEnumerable<LoginRecord>> GetByIpAddressAsync(string ipAddress, int page = 1, int pageSize = 20);

        /// <summary>計算用戶在特定時間內的失敗登入次數</summary>
        Task<int> CountFailedLoginsAsync(Guid userId, DateTime since);

        /// <summary>計算IP在特定時間內的失敗登入次數</summary>
        Task<int> CountFailedLoginsByIpAsync(string ipAddress, DateTime since);

        /// <summary>取得用戶最後一次成功登入記錄</summary>
        Task<LoginRecord?> GetLastSuccessfulLoginAsync(Guid userId);

        /// <summary>取得異常登入記錄（不同地點、設備等）</summary>
        Task<IEnumerable<LoginRecord>> GetAnomalousLoginsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>清理過期登入記錄</summary>
        Task CleanupExpiredRecordsAsync(DateTime expiredBefore);

        /// <summary>取得登入統計</summary>
        Task<Dictionary<string, int>> GetLoginStatsAsync(DateTime startDate, DateTime endDate);
    }
}