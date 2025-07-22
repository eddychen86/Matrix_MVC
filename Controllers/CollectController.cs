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
                new CollectItemViewModel
                {
                    Title = "未來科技圖像",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "小西瓜",
                    CollectedAt = DateTime.Now.AddDays(-1)
                },
                new CollectItemViewModel
                {
                    Title = "數位宇宙奇想",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "大豬豬",
                    CollectedAt = DateTime.Now.AddDays(-2)
                },
                new CollectItemViewModel
                {
                    Title = "區塊鏈紀實",
                    ImageUrl = Url.Content("~/static/img/Cute.png"),
                    AuthorName = "笑笑貓",
                    CollectedAt = DateTime.Now.AddDays(-3)
                }
            };

            return View(fakeData); // 把資料傳給 Collect.cshtml
        }
    }
}
