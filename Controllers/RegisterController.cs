using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Matrix.DTOs;

namespace Matrix.Controllers
{
    public class RegisterController() : WebControllerBase
    {
        /// <summary>顯示註冊頁面</summary>
        [HttpGet, Route("/register")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }
    }
}