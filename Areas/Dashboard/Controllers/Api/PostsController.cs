using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Matrix.DTOs;
using Matrix.Data;
using System;
using static Matrix.DTOs.ArticleDto;

namespace Matrix.Areas.Dashboard.Controllers.Api
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
        public async Task<IActionResult> GetArticles(int page = 1, int pagesize = 10, string? keyword = null, int? status = null, string? date = null)
        {
            if (status is < 0 or > 1) status = null;

            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if(DateTime.TryParse(date, out var d))
                {
                    dateFrom = d.Date;
                    dateTo = d.Date.AddDays(1);
                }
            }
            var (articles, totalCount) = await _articleService.GetArticlesAsync(
            page, pagesize, keyword, authorId: null, status: status, onlyPublic: false, dateFrom:dateFrom, dateTo:dateTo);
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
            var success = await _articleService.AdminUpdateArticleContentAsync(id, dto.Content);
            if (!success)
                return NotFound();

            return Ok(new { success = true });
        }

        //更改文章狀態
        [HttpPatch("status/{id}")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            if (dto is null) return BadRequest("Body required");
            if (dto.Status is < 0 or > 1) return BadRequest("status must be 0 or 1");

            var ok = await _articleService.UpdateStatusAsync(id, dto.Status);
            if (!ok) return NotFound();

            return Ok(new { success = true, status = dto.Status });
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
