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
        private readonly ILogger<PostController> _logger;
        private readonly IPraiseService _praiseService;
        private readonly ICollectService _collectService;
        private readonly IReplyService _replyService;
        private readonly IArticleService _articleService;

        public PostController(
            ILogger<PostController> logger,
            IPraiseService praiseService,
            ICollectService collectService,
            IReplyService replyService,
            IArticleService articleService
        )
        {
            _logger = logger;
            _praiseService = praiseService;
            _collectService = collectService;
            _replyService = replyService;
            _articleService = articleService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "PostController is working", timestamp = DateTime.UtcNow });
        }

        [HttpPost("")]
        public async Task<IActionResult> GetAllPosts([FromBody] GetAllPostsRequestDto request)
        {
            try
            {
                _logger.LogInformation("GetAllPosts called - Page: {Page}, AuthorId: {AuthorId}", 
                    request?.Page ?? 0, request?.AuthorId);

                if (request == null)
                {
                    _logger.LogWarning("Request is null");
                    return BadRequest(new { error = "Request body is required" });
                }

                _logger.LogInformation("About to call GetArticlesAsync with PageSize: {PageSize}", request.PageSize);
                var result = await _articleService.GetArticlesAsync(request.Page, request.PageSize, null, request.AuthorId);
                _logger.LogInformation("GetArticlesAsync completed successfully");
                
                _logger.LogInformation("GetArticlesAsync returned - Articles count: {Count}, Total: {Total}", 
                    result.Articles?.Count ?? 0, result.TotalCount);

                var response = new
                {
                    articles = result.Articles,
                    totalCount = result.TotalCount
                };

                _logger.LogInformation("About to return OK response");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPosts: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                
                return StatusCode(500, new { 
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray()
                });
            }
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
