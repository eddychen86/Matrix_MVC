using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class EditProfilePopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}