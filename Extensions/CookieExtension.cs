
namespace Matrix.Extensions
{
    // TODO: 認證資訊類別，包含使用者驗證狀態
    public class AuthInfo
    {
        public bool IsAuthenticated { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public int? Role { get; set; }
        public bool Guest { get; set; }
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
                UserId = context.Items["UserId"] as Guid?,
                UserName = context.Items["UserName"] as string,
                Role = context.Items["UserRole"] as int?,
                Guest = context.Items["IsGuest"] as bool? ?? false
            };
        }
    }
}
