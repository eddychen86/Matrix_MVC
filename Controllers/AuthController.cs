using Microsoft.AspNetCore.Mvc;
using Matrix.ViewModels;
using Matrix.DTOs;
using Matrix.Services;
using Matrix.Services.Interfaces;
using System.Threading.Tasks;
using Matrix.Models;

namespace Matrix.Controllers
{
    public class AuthController(IUserService _userService) : Controller
    {

        [HttpGet]
        [Route("/register")]
        public ActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
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

            // 創建 CreateUserDto
            var createUserDto = new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email ?? "example@mail.com", // 暫時處理，實際應該要求輸入 Email
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm ?? model.Password
            };

            // 使用 UserService 創建使用者
            var userId = await _userService.CreateUserAsync(createUserDto);
            if (userId == null)
            {
                ModelState.AddModelError("UserName", "Username or email already exists.");
                return View("~/Views/Auth/Register.cshtml", model);
            }

            // 註冊成功後，重定向到登入頁面
            return RedirectToAction("login");
        }

        [HttpGet]
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

            // 使用 UserService 驗證使用者
            var isValid = await _userService.ValidateUserAsync(model.UserName, model.Password);
            if (!isValid)
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

            // 取得使用者資料以判斷角色
            var userDto = await _userService.GetUserByEmailAsync(model.UserName);
            if (userDto == null)
            {
                userDto = await _userService.GetUserByEmailAsync(model.UserName); // 先嘗試 Email，如果沒有再嘗試其他方式
            }

            return Json(new
            {
                success = true,
                redirectUrl = userDto?.Role == 1 ? Url.Action("Index", "Admin") : Url.Action("Index", "Home")
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
