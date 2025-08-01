using Microsoft.AspNetCore.Mvc;
using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
            

            var collects = _context.PraiseCollects
               .Where(p => p.Type == 1) // 只撈收藏的
               .OrderByDescending(p => p.CreateTime) // 收藏時間排序
               .Take(30) // 最多取 30 筆
               .Select(p => new CollectItemViewModel
               {
                   Title = p.Article.Content.Substring(0, 10), // 取文章前10字為標題
                   ImageUrl = "https://i.imgur.com/GD4d09R.png", // 暫時共用圖片
                   AuthorName = p.User.DisplayName ?? "匿名", // 顯示作者名稱
                   CollectedAt = p.CreateTime // 收藏時間
               })
               .ToList();
               
             // 傳遞到 View
             return View(collects);
        }

        // API endpoint for popup data
        [HttpGet]
        [Route("/api/Collects")]
        public IActionResult GetCollectsData()
        {
            // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("36a9c596-b298-49b5-8300-7c3479aed145");

            var collects = _context.PraiseCollects
                .Include(p => p.Article) // 載入文章資料
                 .ThenInclude(a => a.Attachments) // 載入文章作者
                .Where(p => p.Type == 1)
                .OrderByDescending(p => p.CreateTime)
                .Take(30)
                .ToList()
                .Select(p => new CollectItemViewModel
                {
                    Title = p.Article.Content.Substring(0, 10),
                    ImageUrl = p.Article.Attachments.Where(a=>a.Type=="image").Select(a => a.FilePath).FirstOrDefault() ?? Url.Content("~/static/img/Cute.png"), // 使用文章的第一張圖片或預設圖片
                    AuthorName = p.User.DisplayName ?? "匿名",
                    CollectedAt = p.CreateTime
                })
                .ToList();

            return Json(collects);
        }
    }
}
