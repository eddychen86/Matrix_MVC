using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class CommonController : Controller
    {
        public static MenuViewModel BuildMenuModel(HttpContext context)
        {
            var auth = context.GetAuthInfo();
            var isLogin = auth.IsAuthenticated;
            var isAdmin = auth.Role >= 1;

            return new MenuViewModel
            {
                IsAuthenticated = isLogin,
                UserName = auth.UserName.Length > 8 ? auth.UserName.Substring(0, 8) + "..." : auth.UserName,
                UserRole = auth.Role,
                UserId = auth.UserId,
                IsGuest = !isLogin,
                UserImg = auth.AvatarPath,
                Menus = new[]
                {
                    new MenuItemModel { Title = "Search", Icon = "search" },
                    new MenuItemModel { Title = "Notify", Icon = "bell" },
                    new MenuItemModel { Title = "Follows", Icon = "share-2" },
                    new MenuItemModel { Title = "Collects", Icon = "bookmark" },
                },
                Dashboards = !isAdmin ? [] : new[] {
                    new MenuItemModel { Title = "Overview", Icon = "layout-dashboard"},
                    new MenuItemModel { Title = "Users", Icon = "user"},
                    new MenuItemModel { Title = "Posts", Icon = "file-text"},
                    new MenuItemModel { Title = "Reports", Icon = "flag"},
                    new MenuItemModel { Title = "Config", Icon = "bug"},
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