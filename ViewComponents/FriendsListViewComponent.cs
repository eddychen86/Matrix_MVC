using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class FriendsListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}