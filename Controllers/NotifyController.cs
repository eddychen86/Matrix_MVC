using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
    public class NotifyController : Controller
    {
        public IActionResult Index()
        {
            // 只回 View，資料靠 /api/notify 由前端 fetch
            return View();
        }
    }
}
