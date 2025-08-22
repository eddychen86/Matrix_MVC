using Matrix.DTOs;
using Matrix.Models;
using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(
        ILogger<PostController> logger,
        IPraiseService praiseService,
        ICollectService collectService,
        IReplyService replyService,
        IArticleService articleService,
        IUserService userService
    ) : ControllerBase
    {
        private readonly ILogger<PostController> _logger = logger;
        private readonly IPraiseService _praiseService = praiseService;
        private readonly ICollectService _collectService = collectService;
        private readonly IReplyService _replyService = replyService;
        private readonly IArticleService _articleService = articleService;
        private readonly IUserService _userService = userService;
        [HttpGet("hot")]
        public async Task<IActionResult> GetHot([FromQuery] int count = 10)
        {
            try
            {
                var limit = Math.Max(1, Math.Min(count, 50));
                var list = await _articleService.GetPopularArticlesAsync(limit);

                var items = list.Select(a => new
                {
                    articleId = a.ArticleId,
                    content = a.Content,
                    createTime = a.CreateTime,
                    praiseCount = a.PraiseCount,
                    collectCount = a.CollectCount,
                    authorId = a.AuthorId,
                    authorName = a.AuthorName,
                    authorAvatar = a.AuthorAvatar,
                    image = a.Attachments
                        .FirstOrDefault(att => string.Equals(att.Type, "image", StringComparison.OrdinalIgnoreCase))
                });

                return Ok(new { items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hot posts");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> GetAllPosts(
            [FromBody] GetAllPostsRequestDto request,
            [FromQuery] Guid? uid = null)
        {
            try
            {
                _logger.LogInformation("\nGetAllPosts called - Page: {Page}, PageSize: {PageSize}, Uid: {Uid}\n",
                    request?.Page ?? 0, request?.PageSize ?? 20, uid);

                if (request == null)
                {
                    _logger.LogWarning("\nRequest is null\n");
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
                            // 需要將 UserId 轉換為 PersonId（透過 UserService 取得 Profile）
                            var profile = await _userService.GetProfileByIdAsync(currentUserId.Value);
                            if (profile != null)
                                authorId = profile.PersonId;
                        }
                    }
                }

                // 訪客限制：未登入最多 10 筆，且固定僅傳回第一頁
                const int GuestArticleLimit = 10;
                var requestedPageNumber = Math.Max(1, request.Page + 1);
                var requestedPageSize = request.PageSize;

                // 判斷是否已登入（Cookie 中間件會設置 HttpContext.Items["UserId"]）
                var isAuthenticated = User?.Identity?.IsAuthenticated == true ||
                                      (HttpContext.Items["UserId"] as Guid?).HasValue;

                // 訪客：僅允許首次呼叫返回第一批資料；之後一律要求登入
                var pageNumber = isAuthenticated ? requestedPageNumber : 1;
                var pageSize = isAuthenticated ? requestedPageSize : Math.Min(GuestArticleLimit, requestedPageSize);

                if (!isAuthenticated)
                {
                    var guestFirstLoadDone = string.Equals(Request.Cookies["GuestFirstLoad"], "1", StringComparison.Ordinal);

                    // 僅針對「第二頁以後」進行限制。
                    // 允許第 1 頁在同一段期間內被多次請求（避免舊 Cookie 造成首次進站就被擋 403）。
                    if (guestFirstLoadDone && requestedPageNumber > 1)
                    {
                        return StatusCode(403, new
                        {
                            requireLogin = true,
                            message = "請登入以繼續瀏覽更多內容"
                        });
                    }

                    if (!guestFirstLoadDone)
                    {
                        // 標記訪客已取得第一批資料（存於 Cookie）
                        Response.Cookies.Append("GuestFirstLoad", "1", new CookieOptions
                        {
                            HttpOnly = false,
                            IsEssential = true,
                            SameSite = SameSiteMode.Lax,
                            Path = "/",
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });
                    }
                }

                var result = await _articleService.GetArticlesAsync(
                    pageNumber,
                    pageSize,
                    null, // searchKeyword
                    authorId
                );

                var articles = result.Articles;
                var totalCount = isAuthenticated ? result.TotalCount : Math.Min(result.TotalCount, GuestArticleLimit);

                var response = new
                {
                    articles = articles.Select(a => new
                    {
                        articleId = a.ArticleId,
                        content = a.Content,
                        createTime = a.CreateTime,
                        praiseCount = a.PraiseCount,
                        collectCount = a.CollectCount,
                        authorName = a.Author?.DisplayName ?? "未知作者",
                        authorAvator = a.Author?.AvatarPath ?? "",
                        attachments = a.Attachments ?? new List<ArticleAttachmentDto>(),
                        hashtags = a.Hashtags ?? []
                    }).ToList(),
                    totalCount
                };

                _logger.LogInformation("\nAbout to return OK response\n");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\nError in GetAllPosts: {Message}\n", ex.Message);
                _logger.LogError("\nStack trace: {StackTrace}\n", ex.StackTrace);

                return StatusCode(500, new
                {
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Getarticle(Guid id)
        {
            try
            {
                var article = await _articleService.GetArticleDetailAsync(id);

                if (article == null)
                    return NotFound(new { message = "文章不存在" });

                var response = new
                {
                    articleId = article.ArticleId,
                    content = article.Content,
                    createTime = article.CreateTime,
                    praiseCount = article.PraiseCount,
                    collectCount = article.CollectCount,
                    authorName = article.Author?.DisplayName,
                    authorAvatar = article.Author?.AvatarPath,
                    attachments = article.Attachments ?? new List<ArticleAttachmentDto>(),
                    replies = article.Replies ?? new List<ReplyDto>(),
                    hashtags = article.Hashtags ?? new List<HashtagDto>()
                };

                return Ok(new { success = true, article = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching article detail {Id}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
