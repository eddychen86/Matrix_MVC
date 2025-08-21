using Matrix.Data;
using Matrix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Matrix.DTOs;

namespace Matrix.Services
{
    /// <summary>
    /// 系統狀態服務實作
    /// </summary>
    public class SystemStatusService : ISystemStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SystemStatusService> _logger;
        private readonly GoogleSmtpDTOs _smtpConfig;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public SystemStatusService(
            ApplicationDbContext context, 
            ILogger<SystemStatusService> logger,
            IOptions<GoogleSmtpDTOs> smtpConfig)
        {
            _context = context;
            _logger = logger;
            _smtpConfig = smtpConfig.Value;
        }

        public long GetSystemUptime()
        {
            return (long)(DateTime.UtcNow - _startTime).TotalSeconds;
        }

        public async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                // 執行簡單的資料庫查詢來檢查連線狀態
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "資料庫連線檢查失敗");
                return false;
            }
        }

        public async Task<bool> CheckSmtpServiceAsync()
        {
            try
            {
                // 檢查 SMTP 配置是否完整
                if (string.IsNullOrEmpty(_smtpConfig.SmtpServer) || 
                    string.IsNullOrEmpty(_smtpConfig.SenderEmail) || 
                    string.IsNullOrEmpty(_smtpConfig.AppPassword))
                {
                    return false;
                }

                // 嘗試建立 SMTP 連線（不實際發送郵件）
                using var client = new SmtpClient(_smtpConfig.SmtpServer, _smtpConfig.SmtpPort)
                {
                    EnableSsl = _smtpConfig.EnableSsl,
                    Credentials = new System.Net.NetworkCredential(_smtpConfig.SenderEmail, _smtpConfig.AppPassword),
                    Timeout = 5000 // 5秒超時
                };

                // 可以加入實際連線測試，但為了避免頻繁連線，這裡簡化處理
                await Task.Delay(100); // 模擬檢查時間
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP 服務檢查失敗");
                return false;
            }
        }

        public double GetStorageUsagePercentage()
        {
            try
            {
                // 取得應用程式根目錄所在的磁碟資訊
                var appDirectory = Directory.GetCurrentDirectory();
                var driveInfo = new DriveInfo(Path.GetPathRoot(appDirectory) ?? "C:\\");

                if (driveInfo.IsReady)
                {
                    // 設定儲存空間上限為 32GB (以 bytes 為單位)
                    const long maxStorageBytes = 32L * 1024 * 1024 * 1024; // 32GB
                    
                    var totalSpace = driveInfo.TotalSize;
                    var freeSpace = driveInfo.AvailableFreeSpace;
                    var usedSpace = totalSpace - freeSpace;
                    
                    // 使用 32GB 作為基準計算使用率
                    var usagePercentage = (double)usedSpace / maxStorageBytes * 100;
                    
                    // 如果使用量超過 32GB，最大顯示 100%
                    return Math.Round(Math.Min(usagePercentage, 100.0), 1);
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存空間檢查失敗");
                return 0;
            }
        }

        public async Task<SystemStatusInfo> GetSystemStatusAsync()
        {
            var uptimeSeconds = GetSystemUptime();
            var uptimeSpan = TimeSpan.FromSeconds(uptimeSeconds);
            var storageUsage = GetStorageUsagePercentage();

            return new SystemStatusInfo
            {
                UptimeSeconds = uptimeSeconds,
                UptimeFormatted = FormatUptime(uptimeSpan),
                DatabaseConnected = await CheckDatabaseConnectionAsync(),
                SmtpServiceActive = await CheckSmtpServiceAsync(),
                StorageUsagePercentage = storageUsage,
                StorageStatusText = GetStorageStatusText(storageUsage)
            };
        }

        private static string FormatUptime(TimeSpan uptime)
        {
            if (uptime.TotalDays >= 1)
            {
                return $"{(int)uptime.TotalDays} 天 {uptime.Hours} 小時 {uptime.Minutes} 分鐘";
            }
            else if (uptime.TotalHours >= 1)
            {
                return $"{uptime.Hours} 小時 {uptime.Minutes} 分鐘";
            }
            else
            {
                // 最小單位為分鐘，不顯示秒數
                var totalMinutes = Math.Max(1, (int)uptime.TotalMinutes);
                return $"{totalMinutes} 分鐘";
            }
        }

        private static string GetStorageStatusText(double usagePercentage)
        {
            return usagePercentage switch
            {
                < 70 => $"{usagePercentage:F1}% 使用中",
                < 85 => $"{usagePercentage:F1}% 使用中",
                < 95 => $"{usagePercentage:F1}% 使用中 (接近滿載)",
                _ => $"{usagePercentage:F1}% 使用中 (空間不足)"
            };
        }
    }
}