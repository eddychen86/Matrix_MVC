using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IArticleService _articleService;
        
        public PostsController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetArticles(int page=1, int pagesize = 10, string? keyword=null )
        {
            var (articles,totalCount ) = await _articleService.GetArticlesAsync(page, pagesize, keyword);
            return Ok(new
                {
                items = articles,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount/(double)pagesize),
                currentPage = page,
            });
        }

    }
}
