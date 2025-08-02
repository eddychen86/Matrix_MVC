using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class UsersController : Controller
    {
        // GET: UsersController
        public ActionResult Index()
        {
            return View();
        }

    }
}
