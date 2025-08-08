using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.DTOs;
using System.Security.Claims;
using Matrix.Data;
using Microsoft.EntityFrameworkCore;

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
        private readonly ApplicationDbContext _context;

        public PostController(
            ILogger<PostController> logger,
            IPraiseService praiseService,
            ICollectService collectService,
            IReplyService replyService,
            IArticleService articleService,
            ApplicationDbContext context
        )
        {
            _logger = logger;
            _praiseService = praiseService;
            _collectService = collectService;
            _replyService = replyService;
            _articleService = articleService;
            _context = context;
        }

        [HttpPost("")]
        public async Task<IActionResult> GetAllPosts(
            [FromBody] GetAllPostsRequestDto request,
            [FromQuery] Guid? uid = null)
        {
            try
            {
                _logger.LogInformation("GetAllPosts called - Page: {Page}, PageSize: {PageSize}, Uid: {Uid}", 
                    request?.Page ?? 0, request?.PageSize ?? 20, uid);

                if (request == null)
                {
                    _logger.LogWarning("Request is null");
                    return BadRequest(new { error = "Request body is required" });
                }

                Guid? authorId = uid;

                // 如果沒有提供 uid 但是在 profile 頁面且用戶已登入，使用當前用戶 ID
                if (!uid.HasValue)
                {
                    var currentUserId = HttpContext.Items["UserId"] as Guid?;
                    if (currentUserId.HasValue)
                    {
                        // 檢查是否為 Profile 頁面（這裡可以根據需要調整判斷邏輯）
                        var referer = Request.Headers["Referer"].ToString();
                        bool isProfilePage = referer.Contains("/profile", StringComparison.OrdinalIgnoreCase);
                        
                        if (isProfilePage)
                        {
                            // 需要將 UserId 轉換為 PersonId
                            var userPerson = await _context.Persons
                                .Where(p => p.UserId == currentUserId.Value)
                                .FirstOrDefaultAsync();
                            
                            if (userPerson != null)
                            {
                                authorId = userPerson.PersonId;
                                _logger.LogInformation("Using current user's PersonId: {PersonId}", authorId);
                            }
                        }
                    }
                }

                var pageNumber = Math.Max(1, request.Page + 1);

                var result = await _articleService.GetArticlesAsync(
                    pageNumber, 
                    request.PageSize, 
                    null, // searchKeyword
                    authorId
                );
                
                var articles = result.Articles;
                var totalCount = result.TotalCount;

                var response = new
                {
                    articles = articles.Select(a => new
                    {
                        articleId = a.ArticleId,
                        content = a.Content,
                        createTime = a.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                        praiseCount = a.PraiseCount,
                        collectCount = a.CollectCount,
                        authorName = a.Author?.DisplayName ?? "未知作者",
                        authorAvator = a.Author?.AvatarPath ?? "",
                        attachments = a.Attachments ?? new List<ArticleAttachmentDto>()
                    }).ToList(),
                    totalCount = totalCount
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
