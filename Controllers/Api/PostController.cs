using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.DTOs;
using System.Security.Claims;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPraiseService _praiseService;
        private readonly ICollectService _collectService;
        private readonly IReplyService _replyService;
        private readonly ArticleService _articleService;

        public PostController(
            IPraiseService praiseService,
            ICollectService collectService,
            IReplyService replyService,
            ArticleService articleService
        )
        {
            _praiseService = praiseService;
            _collectService = collectService;
            _replyService = replyService;
            _articleService = articleService;
        }

        [HttpPost("")]
        public async Task<IActionResult> GetAllPosts([FromBody] GetAllPostsRequestDto request)
        {
            var result = await _articleService.GetArticlesAsync(request.Page, 20, null, request.AuthorId);
            return Ok(new { 
                articles = result.Articles, 
                totalCount = result.TotalCount 
            });
        }

        [HttpPost("{id}/toggle-praise")]
        public async Task<IActionResult> TogglePraise(Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }
            var result = await _praiseService.TogglePraiseAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("{id}/toggle-collect")]
        public async Task<IActionResult> ToggleCollect(Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }
            var result = await _collectService.ToggleCollectAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(Guid id, [FromBody] ReplyDto model)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }
            var result = await _replyService.CreateReplyAsync(id, userId, model.Content);
            return Ok(result);
        }

        [HttpGet("{id}/replies")]
        public async Task<IActionResult> GetReplies(Guid id)
        {
            var result = await _replyService.GetRepliesByArticleIdAsync(id);
            return Ok(result);
        }
    }
}
