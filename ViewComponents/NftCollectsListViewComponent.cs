using Microsoft.AspNetCore.Mvc;

namespace Matrix.ViewComponents
{
    public class NftCollectsListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
