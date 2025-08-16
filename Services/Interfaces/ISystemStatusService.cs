namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 系統狀態服務介面
    /// </summary>
    public interface ISystemStatusService
    {
        /// <summary>
        /// 取得系統運行時間
        /// </summary>
        /// <returns>系統運行時間（秒）</returns>
        long GetSystemUptime();

        /// <summary>
        /// 檢查資料庫連線狀態
        /// </summary>
        /// <returns>資料庫連線是否正常</returns>
        Task<bool> CheckDatabaseConnectionAsync();

        /// <summary>
        /// 檢查 SMTP 服務狀態
        /// </summary>
        /// <returns>SMTP 服務是否正常</returns>
        Task<bool> CheckSmtpServiceAsync();

        /// <summary>
        /// 取得儲存空間使用情況
        /// </summary>
        /// <returns>儲存空間使用百分比</returns>
        double GetStorageUsagePercentage();

        /// <summary>
        /// 取得完整系統狀態資訊
        /// </summary>
        /// <returns>系統狀態資訊</returns>
        Task<SystemStatusInfo> GetSystemStatusAsync();
    }

    /// <summary>
    /// 系統狀態資訊
    /// </summary>
    public class SystemStatusInfo
    {
        /// <summary>
        /// 系統運行時間（秒）
        /// </summary>
        public long UptimeSeconds { get; set; }

        /// <summary>
        /// 系統運行時間格式化字串
        /// </summary>
        public string UptimeFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 資料庫連線狀態
        /// </summary>
        public bool DatabaseConnected { get; set; }

        /// <summary>
        /// SMTP 服務狀態
        /// </summary>
        public bool SmtpServiceActive { get; set; }

        /// <summary>
        /// 儲存空間使用百分比
        /// </summary>
        public double StorageUsagePercentage { get; set; }

        /// <summary>
        /// 儲存空間使用狀態描述
        /// </summary>
        public string StorageStatusText { get; set; } = string.Empty;
    }
}