using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
