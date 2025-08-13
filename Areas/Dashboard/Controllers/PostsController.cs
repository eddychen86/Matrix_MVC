using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;
using Matrix.Controllers;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [AdminAuthorization] // 需要管理員權限 (Role >= 1)
    public class PostsController : Controller
    {
        // GET: PostsController
        public ActionResult Index()
        {
            // 傳遞 MenuViewModel 給 Layout
            ViewBag.Menu = CommonController.BuildMenuModel(HttpContext);
            return View();
        }

        // GET: Dashboard/Posts/Partial - AJAX 載入
        [HttpGet]
        [Route("Dashboard/Posts/Partial")]
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
