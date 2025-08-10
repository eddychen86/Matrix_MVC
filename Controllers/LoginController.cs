using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>顯示登入頁面</summary>
        [HttpGet, Route("/login")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }
    }
}