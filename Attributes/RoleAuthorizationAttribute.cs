using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Matrix.Extensions;
using Matrix.Services.Interfaces;

namespace Matrix.Attributes
{
    /// <summary>
    /// 自定義角色權限驗證屬性
    /// </summary>
    public class RoleAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly int _minimumRole;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="minimumRole">最低權限等級 (0: 一般會員, 1: 管理員)</param>
        public RoleAuthorizationAttribute(int minimumRole)
        {
            _minimumRole = minimumRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authInfo = context.HttpContext.GetAuthInfo();
            var isApiRequest = IsApiRequest(context.HttpContext);

            // 調試：輸出認證資訊（僅在開發環境）
            // Console.WriteLine($"[AUTH DEBUG] Path: {context.HttpContext.Request.Path}");

            // 檢查是否已認證
            if (!authInfo.IsAuthenticated)
            {
                // Console.WriteLine($"[AUTH DEBUG] User not authenticated, returning 401");
                // 檢查是否為 API 請求
                if (isApiRequest)
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "未授權，請先登入" });
                }
                else
                {
                    // 未登入：重導向到登入頁面
                    context.Result = new RedirectResult("/login");
                }
                return;
            }

            // 簡化的權限檢查，避免異步問題
            // 直接使用從 JWT 中間件取得的資訊進行檢查
            if (authInfo.Role < _minimumRole)
            {
                // Console.WriteLine($"[AUTH DEBUG] Role insufficient, returning 403");
                if (isApiRequest)
                {
                    // API 請求返回 JSON 錯誤
                    context.Result = new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
                }
                else
                {
                    // Web 請求進行重導向
                    if (_minimumRole >= 1)
                    {
                        // 管理員權限不足，重導向到一般用戶頁面
                        context.Result = new RedirectResult("/home/index");
                    }
                    else
                    {
                        // 一般權限不足，重導向到登入頁面
                        context.Result = new RedirectResult("/login");
                    }
                }
                return;
            }
            // Console.WriteLine($"[AUTH DEBUG] Authorization passed");

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 檢查是否為 API 請求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>是否為 API 請求</returns>
        private static bool IsApiRequest(HttpContext context)
        {
            try
            {
                // 檢查路徑是否以 /api/ 開頭
                return context.Request.Path.StartsWithSegments("/api");
            }
            catch (ObjectDisposedException)
            {
                // 如果上下文已釋放，預設為非 API 請求
                return false;
            }
        }
    }

    /// <summary>
    /// 一般會員權限 (Role >= 0)
    /// </summary>
    public class MemberAuthorizationAttribute : RoleAuthorizationAttribute
    {
        public MemberAuthorizationAttribute() : base(0) { }
    }

    /// <summary>
    /// 管理員權限 (Role >= 1)
    /// </summary>
    public class AdminAuthorizationAttribute : RoleAuthorizationAttribute
    {
        public AdminAuthorizationAttribute() : base(1) { }
    }

    /// <summary>
    /// 允許訪客存取的屬性（無權限要求）
    /// </summary>
    public class AllowGuestAttribute : ActionFilterAttribute
    {
        // 此屬性不執行任何權限檢查，允許所有用戶（包括未登入）訪問
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}