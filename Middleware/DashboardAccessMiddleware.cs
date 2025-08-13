using Matrix.Extensions;

namespace Matrix.Middleware
{
    /// <summary>
    /// Dashboard Area 訪問權限檢查中介軟體
    /// 確保只有 Role >= 1 的用戶才能訪問 Dashboard Area
    /// </summary>
    public class DashboardAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DashboardAccessMiddleware> _logger;

        public DashboardAccessMiddleware(RequestDelegate next, ILogger<DashboardAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // 檢查是否是 Dashboard Area 的請求
            if (path.StartsWith("/dashboard"))
            {
                _logger.LogInformation("Dashboard access attempt: {Path}", path);

                // 獲取用戶認證資訊
                var authInfo = context.GetAuthInfo();

                // 檢查是否已認證
                if (!authInfo.IsAuthenticated)
                {
                    _logger.LogWarning("Unauthenticated user attempted to access Dashboard: {Path}", path);
                    // AJAX 請求返回 401，讓前端自行導向
                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                    else
                    {
                        context.Response.Redirect("/login");
                        return;
                    }
                }

                // 檢查是否為管理員 (Role >= 1)
                if (authInfo.Role < 1)
                {
                    _logger.LogWarning(
                        "Unauthorized Dashboard access attempt - UserId: {UserId}, Role: {Role}, Path: {Path}",
                        authInfo.UserId,
                        authInfo.Role,
                        path
                    );

                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden");
                        return;
                    }
                    else
                    {
                        // 重導向到首頁
                        context.Response.Redirect("/home/index");
                        return;
                    }
                }

                _logger.LogInformation(
                    "Dashboard access granted - UserId: {UserId}, Role: {Role}, Path: {Path}",
                    authInfo.UserId,
                    authInfo.Role,
                    path
                );
            }

            // 繼續執行下一個中介軟體
            await _next(context);
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// DashboardAccessMiddleware 的擴展方法
    /// </summary>
    public static class DashboardAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseDashboardAccess(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DashboardAccessMiddleware>();
        }
    }
}
