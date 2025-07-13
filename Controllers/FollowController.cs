using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class FollowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Follow()
        {
            return View();
        }
    }
}
