using Microsoft.AspNetCore.Mvc;
using Matrix.Services;

namespace Matrix.Controllers
{
    public class CommonController : Controller
    {
        public static MenuViewModel BuildMenuModel(HttpContext context)
        {
            var auth = context.GetAuthInfo();
            var isLogin = auth.IsAuthenticated;
            var isAdmin = auth.Role >= 1;

            // 安全處理 UserName，避免 null 或空字串導致崩潰
            var safeUserName = string.IsNullOrWhiteSpace(auth.UserName) ? "Guest" : auth.UserName;
            var displayUserName = safeUserName.Length > 8 ? safeUserName.Substring(0, 8) + "..." : safeUserName;

            // 安全處理 AvatarPath
            var safeAvatarPath = string.IsNullOrWhiteSpace(auth.AvatarPath) ? "" : auth.AvatarPath;

            return new MenuViewModel
            {
                IsAuthenticated = isLogin,
                UserName = displayUserName,
                UserRole = auth.Role,
                UserId = auth.UserId,
                IsGuest = !isLogin,
                UserImg = safeAvatarPath,
                Menus = new[]
                {
                    new MenuItemModel { Title = "Search", Icon = "search" },
                    new MenuItemModel { Title = "Notify", Icon = "bell" },
                    new MenuItemModel { Title = "Follows", Icon = "share-2" },
                    new MenuItemModel { Title = "Collects", Icon = "bookmark" },
                },
                Dashboards = !isAdmin ? [] : new[] {
                    new MenuItemModel { Title = "Overview", Icon = "layout-dashboard", Key = "Overview"},
                    new MenuItemModel { Title = "Users", Icon = "user", Key = "Users"},
                    new MenuItemModel { Title = "Posts", Icon = "file-text", Key = "Posts"},
                    new MenuItemModel { Title = "Reports", Icon = "flag", Key = "Reports"},
                    new MenuItemModel { Title = "Config", Icon = "bug", Key = "Config"},
                },
                Bottoms = new[]
                {
                    new MenuItemModel { Title = "Language", Icon = "languages", Click = "toggleLang" },
                    new MenuItemModel { Title = "HideBar", Icon = "panel-left", Click = "toggleSidebar" },
                    new MenuItemModel {
                        Title = isLogin ? "LogOut" : "Login",
                        Icon = isLogin ? "log-out" : "log-in",
                        Click = isLogin ? "logout" : "login"
                    },
                }
            };
        }
    }
}