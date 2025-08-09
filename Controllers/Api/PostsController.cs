using Matrix.DTOs;
using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Matrix.DTOs.ArticleDto;

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

        //查詢文章
        [HttpGet("list")]
        public async Task<IActionResult> GetArticles(int page = 1, int pagesize = 10, string? keyword = null)
        {
            var (articles, totalCount) = await _articleService.GetArticlesAsync(page, pagesize, keyword);
            return Ok(new
            {
                items = articles,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pagesize),
                currentPage = page,
            });
        }

        //編輯文章
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] ArticleDto.UpdateArticleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Content is required");
            var succeess = await _articleService.AdminUpdateArticleContentAsync(id, dto.Content);
            if (!succeess)
                return NotFound();

            return Ok(new { succeess = true });
        }

        //刪除文章
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            var success = await _articleService.AdminDeleteArticleAsync(id);
            if (!success)
                return NotFound();
            return Ok(new { success = true });
        }


    }
}
