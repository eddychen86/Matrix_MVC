using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Controllers
{
    // [MemberAuthorization] // 允許未登入用戶訪問個人資料頁面
    public class ProfileController : Controller
    {
        [HttpGet]
        [Route("/Profile")]
        [Route("/Profile/{username}")]
        public IActionResult Index(string? username = null)
        {
            return View();
        }
    }
}
