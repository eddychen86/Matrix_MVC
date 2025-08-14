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

            // 檢查是否已認證
            if (!authInfo.IsAuthenticated)
            {
                // 未登入：重導向到登入頁面
                context.Result = new RedirectResult("/login");
                return;
            }

            // 使用統一的授權服務進行更完整的檢查
            var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            if (authorizationService != null && authInfo.UserId != Guid.Empty)
            {
                var authResult = await authorizationService.CheckUserAuthorizationAsync(authInfo.UserId, _minimumRole);
                if (!authResult.IsAuthorized)
                {
                    // 根據錯誤類型決定重導向位置
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
                    return;
                }
            }
            else
            {
                // 回退到原來的檢查方式
                if (authInfo.Role < _minimumRole)
                {
                    // 權限不足：返回 403 Forbidden 或重導向到錯誤頁面
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
                    return;
                }
            }

            base.OnActionExecuting(context);
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