using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class DisclaimerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}