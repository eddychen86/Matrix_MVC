using Microsoft.AspNetCore.Mvc;
using Matrix.DTOs;
using Matrix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Matrix.Controllers
{
    [Route("[controller]")]
    public class PostController(
        IArticleService articleService,
        IHashtagRepository hashtagRepository,
        ILogger<PostController> logger
    ) : Controller
    {
        private readonly IArticleService _articleService = articleService;
        private readonly IHashtagRepository _hashtagRepository = hashtagRepository;
        private readonly ILogger<PostController> _logger = logger;
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 取得目前登入使用者（由 JwtCookieMiddleware 設定）
            var currentUserId = HttpContext.Items["UserId"] as Guid?;
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "請先登入後再發文" });
            }

            try
            {
                var result = await _articleService.CreateArticleWithAttachmentsAsync(currentUserId.Value, dto);
                if (result == null)
                {
                    return BadRequest(new { message = "建立文章失敗" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create article failed");
                return StatusCode(500, "An error occurred while creating the article.");
            }
        }


        [HttpGet("GetHashtags")]
        public async Task<IActionResult> GetHashtags()
        {
            var hashtags = await _hashtagRepository.GetAllAsync();
            var data = hashtags.Select(x => new {
                x.TagId,
                x.Content
            });
            return Json(data);
        }
    }
}
