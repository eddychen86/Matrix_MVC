using Microsoft.AspNetCore.Mvc;

namespace Matrix.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostsController : Controller
    {
        // GET: PostsController
        public ActionResult Index()
        {
            return View();
        }

    }
}
