namespace Matrix.ViewModels
{
    public class MenuViewModel
    {
        public MenuItemModel[] Menus { get; set; } = Array.Empty<MenuItemModel>();
        public MenuItemModel[] Dashboards { get; set; } = Array.Empty<MenuItemModel>();
        public MenuItemModel[] Bottoms { get; set; } = Array.Empty<MenuItemModel>();
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public int? UserRole { get; set; }
        public Guid? UserId { get; set; }
        public bool IsGuest { get; set; }
        public string? UserImg { get; set; }
    }

    public class MenuItemModel
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Click { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; // 用於路由的英文鍵值
    }
}