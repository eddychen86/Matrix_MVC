using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
