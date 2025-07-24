using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class CollectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //GET:Collect
        [HttpGet]
        public IActionResult Collect()
        {
            // 寫死的假資料
            var fakeData = new List<CollectItemViewModel>
            {
                new()
                {
                    Title = "test 00",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "小西瓜",
                    CollectedAt = DateTime.Now.AddDays(-1)
                },
                new()
                {
                    Title = "test 01",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "大豬豬",
                    CollectedAt = DateTime.Now.AddDays(-2)
                },
                new()
                {
                    Title = "test 02",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "笑笑貓",
                    CollectedAt = DateTime.Now.AddDays(-3)
                },
                new()
                {
                    Title = "test 03",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "小西瓜",
                    CollectedAt = DateTime.Now.AddDays(-1)
                },
                new()
                {
                    Title = "test 04",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "大豬豬",
                    CollectedAt = DateTime.Now.AddDays(-2)
                },
                new()
                {
                    Title = "test 05",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "笑笑貓",
                    CollectedAt = DateTime.Now.AddDays(-3)
                }
            };

            return View(fakeData); // 把資料傳給 Collect.cshtml
        }

        // API endpoint for popup data
        [HttpGet]
        [Route("/api/collects")]
        public IActionResult GetCollectsData()
        {
            // 同樣的假資料，但返回 JSON
            var fakeData = new List<CollectItemViewModel>
            {
                new()
                {
                    Title = "test 00",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "小西瓜",
                    CollectedAt = DateTime.Now.AddDays(-1)
                },
                new()
                {
                    Title = "test 01",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "大豬豬",
                    CollectedAt = DateTime.Now.AddDays(-2)
                },
                new()
                {
                    Title = "test 02",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "笑笑貓",
                    CollectedAt = DateTime.Now.AddDays(-3)
                },
                new()
                {
                    Title = "test 03",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "小西瓜",
                    CollectedAt = DateTime.Now.AddDays(-1)
                },
                new()
                {
                    Title = "test 04",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "大豬豬",
                    CollectedAt = DateTime.Now.AddDays(-2)
                },
                new()
                {
                    Title = "test 05",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "笑笑貓",
                    CollectedAt = DateTime.Now.AddDays(-3)
                }
            };

            return Json(fakeData); // 返回 JSON 格式
        }
    }
}
