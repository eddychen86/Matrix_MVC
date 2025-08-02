using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
