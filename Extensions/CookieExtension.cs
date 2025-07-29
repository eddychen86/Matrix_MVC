
namespace Matrix.Extensions
{
    public class AuthInfo
    {
        public bool IsAuthenticated { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public int? Role { get; set; }
        public bool Guest { get; set; }
    }

    public static class CookieExtension
    {
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
