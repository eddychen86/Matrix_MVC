using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class PopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}