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

            // 獲取當前語言
            var currentCulture = GetCurrentCulture(context);

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
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Search"), Icon = "search" },
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Notify"), Icon = "bell" },
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Follows"), Icon = "share-2" },
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Collects"), Icon = "bookmark" },
                },
                Dashboards = !isAdmin ? [] : new[] {
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Overview"), Icon = "layout-dashboard", Key = "Overview"},
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Users"), Icon = "user", Key = "Users"},
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Posts"), Icon = "file-text", Key = "Posts"},
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Reports"), Icon = "flag", Key = "Reports"},
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Config"), Icon = "bug", Key = "Config"},
                },
                Bottoms = new[]
                {
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "Language"), Icon = "languages", Click = "toggleLang" },
                    new MenuItemModel { Title = TranslationService.GetTranslation(currentCulture, "HideBar"), Icon = "panel-left", Click = "toggleSidebar" },
                    new MenuItemModel {
                        Title = isLogin ? TranslationService.GetTranslation(currentCulture, "LogOut") : TranslationService.GetTranslation(currentCulture, "Login"),
                        Icon = isLogin ? "log-out" : "log-in",
                        Click = isLogin ? "logout" : "login"
                    },
                }
            };
        }

        private static string GetCurrentCulture(HttpContext context)
        {
            // 從 Cookie 中獲取語言設定
            if (context?.Request.Cookies.ContainsKey(".AspNetCore.Culture") == true)
            {
                var cultureCookie = context.Request.Cookies[".AspNetCore.Culture"];
                if (!string.IsNullOrEmpty(cultureCookie) && cultureCookie.Contains("c="))
                {
                    var culture = cultureCookie.Split('|')[0].Replace("c=", "");
                    return culture;
                }
            }
            
            // 預設返回繁體中文
            return "zh-TW";
        }
    }
}