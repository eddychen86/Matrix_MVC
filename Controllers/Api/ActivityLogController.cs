using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Matrix.DTOs;
using Matrix.Services.Interfaces;
using System.Security.Claims;
using Matrix.Helpers;

namespace Matrix.Controllers.Api
{
    /// <summary>
    /// 管理員活動記錄 API 控制器
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogController : ControllerBase
    {
        private readonly IAdminActivityService _activityService;
        private readonly ILogger<ActivityLogController> _logger;

        public ActivityLogController(
            IAdminActivityService activityService,
            ILogger<ActivityLogController> logger)
        {
            _activityService = activityService;
            _logger = logger;
        }

        /// <summary>
        /// 取得管理員活動記錄（分頁查詢）
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns>分頁活動記錄</returns>
        [HttpPost("search")]
        public async Task<ActionResult<PagedActivityLogDto>> GetActivities([FromBody] ActivityLogFilterDto filter)
        {
            try
            {
                var result = await _activityService.GetActivitiesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢管理員活動記錄時發生錯誤");
                return StatusCode(500, new { Message = "查詢活動記錄時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 取得指定用戶的活動記錄
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>用戶活動記錄</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PagedActivityLogDto>> GetUserActivities(
            Guid userId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _activityService.GetUserActivitiesAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢用戶活動記錄時發生錯誤: UserId={UserId}", userId);
                return StatusCode(500, new { Message = "查詢用戶活動記錄時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 取得活動統計資料
        /// </summary>
        /// <param name="startDate">開始時間</param>
        /// <param name="endDate">結束時間</param>
        /// <returns>統計資料</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<ActivityLogStatsDto>> GetActivityStats(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _activityService.GetActivityStatsAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動統計資料時發生錯誤");
                return StatusCode(500, new { Message = "取得統計資料時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 記錄管理員活動
        /// </summary>
        /// <param name="activityDto">活動記錄資料</param>
        /// <returns>創建的活動記錄 ID</returns>
        [HttpPost("log")]
        public async Task<ActionResult<Guid>> LogActivity([FromBody] CreateActivityLogDto activityDto)
        {
            try
            {
                var activityId = await _activityService.LogActivityAsync(activityDto);
                return Ok(new { ActivityId = activityId, Message = "活動記錄成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員活動時發生錯誤");
                return StatusCode(500, new { Message = "記錄活動時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 記錄管理員登入
        /// </summary>
        /// <returns>記錄結果</returns>
        [HttpPost("login")]
        public async Task<ActionResult<Guid>> LogLogin()
        {
            try
            {
                // 從 JWT Token 或 Session 中獲取用戶資訊
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(new { Message = "無效的用戶資訊" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                var adminName = userNameClaim ?? "Unknown";
                var role = int.TryParse(roleClaim, out var r) ? r : 1;

                var activityId = await _activityService.LogLoginAsync(userId, adminName, role, ipAddress, userAgent);
                
                return Ok(new { ActivityId = activityId, Message = "登入記錄成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員登入時發生錯誤");
                return StatusCode(500, new { Message = "記錄登入時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 記錄管理員登出
        /// </summary>
        /// <returns>記錄結果</returns>
        [HttpPost("logout")]
        public async Task<ActionResult<bool>> LogLogout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(new { Message = "無效的用戶資訊" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var success = await _activityService.LogLogoutAsync(userId, ipAddress);
                
                return Ok(new { Success = success, Message = success ? "登出記錄成功" : "登出記錄失敗" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員登出時發生錯誤");
                return StatusCode(500, new { Message = "記錄登出時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 取得用戶最後成功登入記錄
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <returns>最後登入記錄</returns>
        [HttpGet("user/{userId}/last-login")]
        public async Task<ActionResult<AdminActivityLogDto>> GetLastSuccessfulLogin(Guid userId)
        {
            try
            {
                var result = await _activityService.GetLastSuccessfulLoginAsync(userId);
                if (result == null)
                {
                    return NotFound(new { Message = "找不到登入記錄" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得最後登入記錄時發生錯誤: UserId={UserId}", userId);
                return StatusCode(500, new { Message = "取得登入記錄時發生錯誤", Error = ex.Message });
            }
        }

        /// <summary>
        /// 清理過期的活動記錄
        /// </summary>
        /// <param name="days">保留天數</param>
        /// <returns>清理結果</returns>
        [HttpDelete("cleanup")]
        public async Task<ActionResult<int>> CleanupExpiredRecords([FromQuery] int days = 90)
        {
            try
            {
                var expiredBefore = TimeZoneHelper.GetTaipeiTimeAddDays(-days);
                var cleanedCount = await _activityService.CleanupExpiredRecordsAsync(expiredBefore);
                
                return Ok(new { CleanedCount = cleanedCount, Message = $"成功清理 {cleanedCount} 筆過期記錄" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期記錄時發生錯誤");
                return StatusCode(500, new { Message = "清理過期記錄時發生錯誤", Error = ex.Message });
            }
        }
    }
}