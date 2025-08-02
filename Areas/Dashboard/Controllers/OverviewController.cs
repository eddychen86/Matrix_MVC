using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class OverviewController : Controller
    {
        // GET: OverviewController
        public ActionResult Index()
        {
            return View();
        }

    }
}
