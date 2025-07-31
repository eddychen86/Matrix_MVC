using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        // GET: ReportsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
