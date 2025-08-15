using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class PostListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}