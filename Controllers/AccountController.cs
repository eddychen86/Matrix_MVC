using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    // 1. 引用 EntityFrameworkCore
using Matrix.Data;                      // 2. 引用您的 DbContext 所在的命名空間
using Matrix.Models;                    // 3. 引用您的 User 模型所在的命名空間
using System.Threading.Tasks;

namespace Matrix.Controllers
{
    public class AccountController : Controller
    {
        // 建立一個私有欄位來存放 DbContext 的實例
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("/login")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string UserName, string Password)
        {
            // Logic for user login
            // This is a placeholder; actual implementation will depend on your authentication setup
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            // 非同步地從資料庫中尋找使用者
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName);

            // 檢查密碼
            if (user == null || user.Password != Password)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // 若帳密無誤，則重定向到首頁
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
    }
}
