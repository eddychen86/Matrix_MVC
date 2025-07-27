using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
            // 寫死的假資料
            //var fakeData = new List<CollectItemViewModel>
            //{
            //    new()
            //    {
            //        Title = "test 00",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "小西瓜",
            //        CollectedAt = DateTime.Now.AddDays(-1)
            //    },
            //    new()
            //    {
            //        Title = "test 01",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "大豬豬",
            //        CollectedAt = DateTime.Now.AddDays(-2)
            //    },
            //    new()
            //    {
            //        Title = "test 02",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "笑笑貓",
            //        CollectedAt = DateTime.Now.AddDays(-3)
            //    },
            //    new()
            //    {
            //        Title = "test 03",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "小西瓜",
            //        CollectedAt = DateTime.Now.AddDays(-1)
            //    },
            //    new()
            //    {
            //        Title = "test 04",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "大豬豬",
            //        CollectedAt = DateTime.Now.AddDays(-2)
            //    },
            //    new()
            //    {
            //        Title = "test 05",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "笑笑貓",
            //        CollectedAt = DateTime.Now.AddDays(-3)
            //    }
            //};

            //return View(fakeData); // 把資料傳給 Collect.cshtml

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
        [Route("/api/collects")]
        public IActionResult GetCollectsData()
        {
            // 同樣的假資料，但返回 JSON
            //var fakeData = new List<CollectItemViewModel>
            //{
            //    new()
            //    {
            //        Title = "test 00",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "小西瓜",
            //        CollectedAt = DateTime.Now.AddDays(-1)
            //    },
            //    new()
            //    {
            //        Title = "test 01",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "大豬豬",
            //        CollectedAt = DateTime.Now.AddDays(-2)
            //    },
            //    new()
            //    {
            //        Title = "test 02",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "笑笑貓",
            //        CollectedAt = DateTime.Now.AddDays(-3)
            //    },
            //    new()
            //    {
            //        Title = "test 03",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "小西瓜",
            //        CollectedAt = DateTime.Now.AddDays(-1)
            //    },
            //    new()
            //    {
            //        Title = "test 04",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "大豬豬",
            //        CollectedAt = DateTime.Now.AddDays(-2)
            //    },
            //    new()
            //    {
            //        Title = "test 05",
            //        ImageUrl = Url.Content("~/static/img/Cute.png"),
            //        AuthorName = "笑笑貓",
            //        CollectedAt = DateTime.Now.AddDays(-3)
            //    }
            //};

            //return Json(fakeData); // 返回 JSON 格式

            var collects = _context.PraiseCollects
                .Where(p => p.Type == 1)
                .OrderByDescending(p => p.CreateTime)
                .Take(30)
                .Select(p => new CollectItemViewModel
                {
                    Title = p.Article.Content.Substring(0, 10),
                    ImageUrl = "https://i.imgur.com/GD4d09R.png",
                    AuthorName = p.User.DisplayName ?? "匿名",
                    CollectedAt = p.CreateTime
                })
                .ToList();

            return Json(collects);
        }
    }
}
