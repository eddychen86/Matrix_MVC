using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
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
