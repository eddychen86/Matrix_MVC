using Microsoft.AspNetCore.Mvc;
using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers
{
    public class CollectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollectController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //GET:Collect
        [HttpGet]
        public IActionResult Collect()
        {
             // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("7ffbe594-de54-452a-92b0-311631587369");

            var collects = _context.PraiseCollects
                .Include(i => i.Article)
                .Include(i => i.User)
                .Where(p => p.Type == 1 && p.UserId == currentUserId)        // 只撈收藏的和指定使用者
                .OrderByDescending(p => p.CreateTime)                        // 收藏時間排序
                .Take(30) // 最多取 30 筆
                .ToList()
                .Select(p => new CollectItemViewModel
                {
                    Title = p.Article?.Content?.Substring(0, Math.Min(10, p.Article.Content?.Length ?? 0)) ?? "",        // 取文章前10字為標題
                    ImageUrl = "https://i.imgur.com/GD4d09R.png",            // 暫時共用圖片
                    AuthorName = p.User?.DisplayName ?? "匿名",                // 顯示作者名稱
                    CollectedAt = p.CreateTime                               // 收藏時間
                })
                .ToList();

            // 傳遞到 View
            return View(collects);
        }

        // API endpoint for popup data
        [HttpGet]
        [Route("/api/collects")]
        public IActionResult GetCollectsData()
        {
            // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("7ffbe594-de54-452a-92b0-311631587369");

            var collects = _context.PraiseCollects
                .Include(i => i.Article)
                .Include(i => i.User)
                .Where(p => p.Type == 1 && p.UserId == currentUserId)
                .OrderByDescending(p => p.CreateTime)
                .Take(30)
                .ToList()
                .Select(p => new CollectItemViewModel
                {
                    Title = p.Article?.Content?.Substring(0, Math.Min(10, p.Article.Content?.Length ?? 0)) ?? "",
                    ImageUrl = "https://i.imgur.com/GD4d09R.png",
                    AuthorName = p.User?.DisplayName ?? "匿名",
                    CollectedAt = p.CreateTime
                })
                .ToList();

            return Json(collects);
        }
    }
}
