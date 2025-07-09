using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class CollectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //GET:Collect
        [HttpGet]
        public IActionResult Collect()
        {
            return View();
        }
    }
}
