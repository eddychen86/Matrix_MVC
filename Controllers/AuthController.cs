using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    // 1. 引用 EntityFrameworkCore
using Matrix.Data;                      // 2. 引用您的 DbContext 所在的命名空間
using Matrix.Models;                    // 3. 引用您的 User 模型所在的命名空間
using Matrix.ViewModels;                // 4. 引用您的 ViewModel 所在的命名空間
using System.Threading.Tasks;

namespace Matrix.Controllers
{
    public class AuthController(ApplicationDbContext _context) : Controller
    {
        [HttpGet]
        [Route("/login")]
        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }

        [HttpPost]
        public ActionResult ForgotPwd(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Login.cshtml", model);
            }

            /*
            SMTP 郵件發送邏輯可以在這裡實現
            */

            // 這裡可以添加忘記密碼的邏輯，例如發送重置密碼郵件等
            // 目前僅返回登入頁面
            ModelState.AddModelError("", "Forgot password functionality is not implemented yet.");
            return View("~/Views/Auth/Login.cshtml", model);
        }

        [HttpPost]
        [Route("/login")]
        // [ValidateAntiForgeryToken] // 1. 添加防止 CSRF 攻擊的標記
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Login.cshtml", model);
            }

            // 非同步地從資料庫中尋找使用者
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName || u.Email == model.UserName);

            // 檢查密碼
            if (user == null || user.Password != model.Password)
            {
                Console.WriteLine("Login failed - Invalid credentials");
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("~/Views/Auth/Login.cshtml", model);
            }

            // 若帳密無誤，則重定向到首頁
            return user.Role == 1 ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
        }
    }
}
