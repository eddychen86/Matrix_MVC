using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LogsController : Controller
    {
        // GET: LogsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
