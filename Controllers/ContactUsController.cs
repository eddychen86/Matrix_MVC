using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}