
namespace Matrix.Extensions
{
    // TODO: 認證資訊類別，包含使用者驗證狀態
    public class AuthInfo
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Role { get; set; }
        public string? AvaterPath { get; set; }
    }

    // TODO: HttpContext 擴充方法，用於取得認證資訊
    public static class CookieExtension
    {
        // TODO: 從 HttpContext.Items 中取得使用者認證資訊
        public static AuthInfo GetAuthInfo(this HttpContext context)
        {
            return new AuthInfo
            {
                IsAuthenticated = context.Items["IsAuthenticated"] as bool? ?? false,
                UserId = context.Items["UserId"] as Guid? ?? Guid.Empty,
                UserName = context.Items["UserName"] as string ?? string.Empty,
                Role = context.Items["UserRole"] as int? ?? 0,
                AvaterPath = context.Items["AvaterPath"] as string
            };
        }
    }
}
