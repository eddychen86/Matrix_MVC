using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class CreatePostPopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}