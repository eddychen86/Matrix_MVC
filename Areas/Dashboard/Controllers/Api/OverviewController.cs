using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;
using Matrix.Services.Interfaces;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorization] // 跟頁面一樣，只有管理員可用
    public class OverviewController(
        ILogger<OverviewController> logger,
        IUserService userService,
        IReportService reportService,
        IArticleService articleService,
        IHashtagService hashtagService,
        ISystemStatusService systemStatusService
    ) : ControllerBase
    {
        private readonly ILogger<OverviewController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly IReportService _reportService = reportService;
        private readonly IArticleService _articleService = articleService;
        private readonly IHashtagService _hashtagService = hashtagService;
        private readonly ISystemStatusService _systemStatusService = systemStatusService;

        [HttpGet("GetUserState")]
        public async Task<IActionResult> GetTotalUsers()
        {
            var users = await _userService.GetUserBasicsAsync() ?? [];

            var result = new
            {
                totalUsers = users.Count,  // 使用 Count 屬性而不是 Count() 方法
                users
            };

            return Ok(result);
        }

        [HttpGet("GetReportsState")]
        public async Task<IActionResult> GetPendingReports()
        {
            try
            {
                // 使用 ReportService 獲取待處理的報告數量
                var pendingReportsCount = await _reportService.GetPendingReportsCountAsync();

                var result = new
                {
                    pendingReports = pendingReportsCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports state");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetPostsState")]
        public async Task<IActionResult> GetPostsState()
        {
            try
            {
                // 使用 ArticleService 獲取文章總數
                var totalPosts = await _articleService.GetTotalArticlesCountAsync(onlyPublic: true);

                var result = new
                {
                    totalPosts = totalPosts
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts state");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetHashtagsState")]
        public async Task<IActionResult> GetHashtagsState()
        {
            try
            {
                // 使用 HashtagService 獲取所有標籤及使用次數
                var hashtagsWithUsage = await _hashtagService.GetAllHashtagsWithUsageCountAsync();

                var result = new
                {
                    totalHashtags = hashtagsWithUsage.Count,
                    hashtags = hashtagsWithUsage.Select(h => new
                    {
                        tagId = h.Tag.TagId,
                        name = h.Tag.Content,
                        usageCount = h.UsageCount
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hashtags state");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetSystemStatus")]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                // 使用 SystemStatusService 獲取系統狀態
                var systemStatus = await _systemStatusService.GetSystemStatusAsync();

                var result = new
                {
                    uptimeSeconds = systemStatus.UptimeSeconds,
                    uptimeFormatted = systemStatus.UptimeFormatted,
                    databaseConnected = systemStatus.DatabaseConnected,
                    smtpServiceActive = systemStatus.SmtpServiceActive,
                    storageUsagePercentage = systemStatus.StorageUsagePercentage,
                    storageStatusText = systemStatus.StorageStatusText
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system status");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetSystemUptime")]
        public IActionResult GetSystemUptime()
        {
            try
            {
                var uptimeSeconds = _systemStatusService.GetSystemUptime();
                var uptimeSpan = TimeSpan.FromSeconds(uptimeSeconds);

                var result = new
                {
                    uptimeSeconds = uptimeSeconds,
                    days = (int)uptimeSpan.TotalDays,
                    hours = uptimeSpan.Hours,
                    minutes = uptimeSpan.Minutes
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system uptime");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetDatabaseStatus")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var databaseConnected = await _systemStatusService.CheckDatabaseConnectionAsync();

                var result = new
                {
                    databaseConnected = databaseConnected
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database status");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetSmtpStatus")]
        public async Task<IActionResult> GetSmtpStatus()
        {
            try
            {
                var smtpServiceActive = await _systemStatusService.CheckSmtpServiceAsync();

                var result = new
                {
                    smtpServiceActive = smtpServiceActive
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMTP status");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetStorageStatus")]
        public IActionResult GetStorageStatus()
        {
            try
            {
                var storageUsagePercentage = _systemStatusService.GetStorageUsagePercentage();

                var result = new
                {
                    storageUsagePercentage = storageUsagePercentage
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage status");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

    }
}