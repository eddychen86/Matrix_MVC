using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class LogsController : Controller
    {
        // GET: LogsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
