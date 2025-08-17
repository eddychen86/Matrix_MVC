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

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            var authInfo = context.HttpContext.GetAuthInfo();
            var isApiRequest = IsApiRequest(context.HttpContext);

            // 調試：輸出認證資訊
            Console.WriteLine($"[AUTH DEBUG] Path: {context.HttpContext.Request.Path}");
            Console.WriteLine($"[AUTH DEBUG] IsApiRequest: {isApiRequest}");
            Console.WriteLine($"[AUTH DEBUG] AuthInfo.IsAuthenticated: {authInfo.IsAuthenticated}");
            Console.WriteLine($"[AUTH DEBUG] AuthInfo.Role: {authInfo.Role}");
            Console.WriteLine($"[AUTH DEBUG] AuthInfo.UserId: {authInfo.UserId}");
            Console.WriteLine($"[AUTH DEBUG] MinimumRole Required: {_minimumRole}");

            // 檢查是否已認證
            if (!authInfo.IsAuthenticated)
            {
                Console.WriteLine($"[AUTH DEBUG] User not authenticated, returning 401");
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

            // 使用統一的授權服務進行更完整的檢查
            var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            if (authorizationService != null && authInfo.UserId != Guid.Empty)
            {
                var authResult = await authorizationService.CheckUserAuthorizationAsync(authInfo.UserId, _minimumRole);
                if (!authResult.IsAuthorized)
                {
                    // 根據錯誤類型決定響應
                    if (IsApiRequest(context.HttpContext))
                    {
                        // API 請求返回 JSON 錯誤
                        switch (authResult.StatusCode)
                        {
                            case UserStatusCode.NotFound:
                                context.Result = new NotFoundObjectResult(new { message = "用戶不存在" });
                                break;
                            case UserStatusCode.Unconfirmed:
                                context.Result = new UnauthorizedObjectResult(new { message = "帳戶未啟用" });
                                break;
                            case UserStatusCode.Banned:
                                context.Result = new UnauthorizedObjectResult(new { message = "帳戶已被停用" });
                                break;
                            case UserStatusCode.InsufficientPermission:
                                context.Result = new ObjectResult(new { message = "權限不足" }) { StatusCode = 403 };
                                break;
                            default:
                                context.Result = new UnauthorizedObjectResult(new { message = "授權失敗" });
                                break;
                        }
                    }
                    else
                    {
                        // Web 請求進行重導向
                        switch (authResult.StatusCode)
                        {
                            case UserStatusCode.NotFound:
                            case UserStatusCode.Unconfirmed:
                            case UserStatusCode.Banned:
                                context.Result = new RedirectResult("/login");
                                break;
                            case UserStatusCode.InsufficientPermission:
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
                                break;
                            default:
                                context.Result = new RedirectResult("/login");
                                break;
                        }
                    }
                    return;
                }
            }
            else
            {
                // 回退到原來的檢查方式
                Console.WriteLine($"[AUTH DEBUG] Using fallback role check: {authInfo.Role} >= {_minimumRole}");
                if (authInfo.Role < _minimumRole)
                {
                    Console.WriteLine($"[AUTH DEBUG] Role insufficient, returning 403");
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
                Console.WriteLine($"[AUTH DEBUG] Authorization passed");
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 檢查是否為 API 請求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>是否為 API 請求</returns>
        private static bool IsApiRequest(HttpContext context)
        {
            // 檢查路徑是否以 /api/ 開頭
            return context.Request.Path.StartsWithSegments("/api");
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
}