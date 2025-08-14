using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}