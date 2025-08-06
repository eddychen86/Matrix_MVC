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
        public async Task<IActionResult>Index()
        {
            return View();
        }
    }
}
