using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class ReplyPopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}