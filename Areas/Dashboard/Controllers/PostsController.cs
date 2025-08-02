using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class PostsController : Controller
    {
        // GET: PostsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
