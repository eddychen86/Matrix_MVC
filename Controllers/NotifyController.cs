using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class NotifyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //GET:Notify
        [HttpGet]
        public IActionResult Notify()
        {
            return View();
        }
    }
}
