using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;
using Matrix.Services.Interfaces;
using Matrix.Services;

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [AdminAuthorization] // 需要管理員權限 (Role >= 1)
    public class PostsController : Controller
    {
        private readonly IArticleService _articleService;

        public PostsController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        //查詢文章清單
        public async Task<IActionResult> Index(int page = 1, int pagesize = 10, string? keyword = null)
        {
            var (articles, totalCount) = await _articleService.GetArticlesAsync(page, pagesize, keyword);

            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pagesize;
            ViewBag.Keyword = keyword;

            return View(articles);
        }

        // GET : Dashboard/Posts/Partial - AJAX 載入
        [HttpGet]
        [Route("Dashboard/Posts/Partial")]
        public async Task<IActionResult> Partial(int page = 1, int pagesize = 10, string? keyword = null)
        {
            var (articles, totalCount) = await _articleService.GetArticlesAsync(page, pagesize, keyword);
            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pagesize;
            ViewBag.Keywords = keyword;

            return PartialView("Index",articles);
        }

    }
}
