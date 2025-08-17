using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class RegisterController() : Controller
    {
        // TODO:顯示註冊頁面
        [HttpGet, Route("/register")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }
    }
}