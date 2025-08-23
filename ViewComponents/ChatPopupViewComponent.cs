using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class ChatPopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}