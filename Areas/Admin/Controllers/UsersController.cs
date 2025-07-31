using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        // GET: UsersController
        public ActionResult Index()
        {
            return View();
        }

    }
}
