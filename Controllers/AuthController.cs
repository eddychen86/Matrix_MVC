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
        [Route("/register")]
        public ActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Register.cshtml", model);
            }

            // 檢查使用者是否已存在
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
                return View("~/Views/Auth/Register.cshtml", model);
            }
            else
            {
                // 創建新使用者
                var newUser = new User
                {
                    UserName = model.UserName,
                    Password = model.Password, // 注意：實際應用中應該對密碼進行加密處理
                    Role = 0, // 預設角色為普通使用者
                    CreateTime = DateTime.Now,
                    Person = null
                };

                // 將新使用者加入資料庫
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 註冊成功後，重定向到登入頁面
                return RedirectToAction("Login");
            }
        [Route("/login")]
        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [Route("/login")]
        public ActionResult LoginPost(LoginViewModel model)
        {
            // 備援處理：當 JavaScript 失效時的表單提交
            ModelState.AddModelError("", "Please enable JavaScript for proper login functionality.");
            return View("~/Views/Auth/Login.cshtml", model);
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<IActionResult> LoginApi([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return Json(new { success = false, errors });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == model.UserName || u.Email == model.UserName);

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError("", "Invalid user name or password.");
                var errors = ModelState
                   .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                   .ToDictionary(
                       kvp => kvp.Key,
                       kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                   );
                return Json(new { success = false, errors });
            }

            return Json(new
            {
                success = true,
                redirectUrl = user.Role == 1 ? Url.Action("Index", "Admin") : Url.Action("Index", "Home")
            });
        }

        [HttpPost]
        [Route("/login/forgot")]
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
    }
}
