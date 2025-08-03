using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [AdminAuthorization] // 需要管理員權限 (Role >= 1)
    public class UsersController : Controller
    {
        // GET: UsersController
        public ActionResult Index()
        {
            return View();
        }

        // GET: Dashboard/Users/Partial - AJAX 載入
        [HttpGet]
        [Route("Dashboard/Users/Partial")]
        public ActionResult Partial()
        {
            // 如果是 AJAX 請求，返回 Partial View
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Index");
            }
            
            // 否則重導向到完整頁面
            return RedirectToAction("Index");
        }
    }
}
