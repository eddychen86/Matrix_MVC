using Microsoft.AspNetCore.Mvc;
using Matrix.Extensions;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;

namespace Matrix.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IStringLocalizer<MenuViewComponent> _localizer;
        private readonly IUserService _userService;

        public MenuViewComponent(IStringLocalizer<MenuViewComponent> localizer, IUserService userService)
        {
            _localizer = localizer;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 使用 CookieExtension 取得認證資訊
            var authInfo = HttpContext.GetAuthInfo();

            var menus = new[]
            {
                new MenuItemModel { Title = _localizer["Search"], Icon = "search", Func = "" },
                new MenuItemModel { Title = _localizer["Notify"], Icon = "bell", Func = "" },
                new MenuItemModel { Title = _localizer["Follows"], Icon = "share-2", Func = "" },
                new MenuItemModel { Title = _localizer["Collects"], Icon = "bookmark", Func = "" },
            };

            var bottoms = new[]
            {
                new MenuItemModel { Title = _localizer["Language"], Icon = "languages", Click = "window.toggleLang()" },
                new MenuItemModel { Title = _localizer["HideBar"], Icon = "panel-right-open", Click = "window.toggleSidebar()" },
                new MenuItemModel { 
                    Title = authInfo.IsAuthenticated ? _localizer["LogOut"] : _localizer["Login"], 
                    Icon = authInfo.IsAuthenticated ? "log-out" : "log-in", 
                    Click = authInfo.IsAuthenticated ? "window.logout()" : "window.location.href='/login'" 
                },
            };

            // 取得用戶頭像
            var userImg = !authInfo.UserId.HasValue
                ? null
                : (await _userService.GetUserAsync(authInfo.UserId.Value))?.Person?.AvatarPath;

            var model = new MenuViewModel
            {
                Menus = menus,
                Bottoms = bottoms,
                IsAuthenticated = authInfo.IsAuthenticated,
                UserName = authInfo.UserName,
                UserRole = authInfo.Role,
                IsGuest = authInfo.Guest,
                UserImg = userImg
            };

            return View(model);
        }
    }
}