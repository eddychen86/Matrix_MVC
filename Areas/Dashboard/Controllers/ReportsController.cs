using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class ReportsController : Controller
    {
        // GET: ReportsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
