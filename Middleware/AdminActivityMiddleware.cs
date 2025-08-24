using Matrix.Services.Interfaces;
using System.Security.Claims;

namespace Matrix.Middleware
{
    /// <summary>
    /// 管理員活動記錄中間件
    /// 自動記錄 role >= 1 使用者的頁面訪問和重要操作
    /// </summary>
    public class AdminActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminActivityMiddleware> _logger;
        
        // 需要記錄活動的路徑清單
        private readonly HashSet<string> _trackedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/dashboard",
            "/dashboard/overview",
            "/dashboard/config",
            "/dashboard/users", 
            "/dashboard/posts",
            "/dashboard/reports"
        };

        public AdminActivityMiddleware(
            RequestDelegate next, 
            IServiceProvider serviceProvider,
            ILogger<AdminActivityMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 檢查是否為管理員用戶
            var userRole = context.Items["UserRole"] as int?;
            var userId = context.Items["UserId"] as Guid?;
            var userName = context.Items["UserName"] as string;

            // TODO(human): 增加更穩健的驗證邏輯
            // 確保所有必需值都有效，包含非預設值檢查和更嚴格的 null 檢查
            var shouldTrack = userRole >= 1 && 
                             userId.HasValue && 
                             !string.IsNullOrEmpty(userName) &&
                             ShouldTrackPath(context.Request.Path);

            DateTime startTime = DateTime.Now;

            try
            {
                await _next(context);

                // 在請求完成後記錄頁面訪問（只記錄成功的請求）
                if (shouldTrack && context.Response.StatusCode < 400 && userId.HasValue && userRole.HasValue)
                {
                    var duration = (int)(DateTime.Now - startTime).TotalSeconds;
                    await LogPageVisitAsync(context, userId.Value, userName!, userRole.Value, duration);
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤活動
                if (shouldTrack && userId.HasValue && userRole.HasValue && !string.IsNullOrEmpty(userName))
                {
                    await LogErrorAsync(context, userId.Value, userName, userRole.Value, ex.Message);
                }
                
                throw; // 重新拋出異常
            }
        }

        /// <summary>
        /// 判斷是否需要追蹤此路徑
        /// </summary>
        private bool ShouldTrackPath(PathString path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var pathStr = path.ToString();
            
            // 排除靜態資源和API狀態檢查
            if (pathStr.StartsWith("/static", StringComparison.OrdinalIgnoreCase) ||
                pathStr.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                pathStr.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                pathStr.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                pathStr.StartsWith("/api/auth/status", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // 檢查是否為需要追蹤的頁面
            return _trackedPaths.Any(trackedPath => 
                pathStr.StartsWith(trackedPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 記錄頁面訪問活動
        /// </summary>
        private async Task LogPageVisitAsync(HttpContext context, Guid userId, string userName, int role, int duration)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var activityService = scope.ServiceProvider.GetRequiredService<IAdminActivityService>();

                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var pagePath = context.Request.Path.ToString();

                await activityService.LogPageVisitAsync(
                    userId, 
                    userName, 
                    role, 
                    pagePath, 
                    ipAddress, 
                    userAgent, 
                    duration
                );

                _logger.LogDebug("Page visit logged for admin {UserName}: {PagePath}", userName, pagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log page visit for user {UserName}", userName);
                // 不要因為記錄失敗而影響正常流程
            }
        }

        /// <summary>
        /// 記錄錯誤活動
        /// </summary>
        private async Task LogErrorAsync(HttpContext context, Guid userId, string userName, int role, string errorMessage)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var activityService = scope.ServiceProvider.GetRequiredService<IAdminActivityService>();

                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var pagePath = context.Request.Path.ToString();

                await activityService.LogErrorAsync(
                    userId, 
                    userName, 
                    role, 
                    errorMessage, 
                    pagePath, 
                    ipAddress, 
                    userAgent
                );

                _logger.LogDebug("Error logged for admin {UserName}: {ErrorMessage}", userName, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log error for user {UserName}", userName);
                // 不要因為記錄失敗而影響正常流程
            }
        }
    }

    /// <summary>
    /// 中間件擴展方法
    /// </summary>
    public static class AdminActivityMiddlewareExtensions
    {
        /// <summary>
        /// 註冊管理員活動記錄中間件
        /// </summary>
        public static IApplicationBuilder UseAdminActivityLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminActivityMiddleware>();
        }
    }
}