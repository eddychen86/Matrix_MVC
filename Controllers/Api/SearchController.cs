using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchUserService _searchUserService;
        private readonly ISearchHashtagService _searchHashtagService;
        private readonly IFollowService _followService;
        public SearchController(ISearchUserService searchUserService, ISearchHashtagService searchHashtagService, IFollowService followService)
        {
            _searchUserService = searchUserService;
            _searchHashtagService = searchHashtagService;
            _followService = followService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return ApiError("關鍵字不能為空");
            }
            var result = await _searchUserService.SearchUsersAsync(keyword);
            return ApiSuccess(result);
        }

        [HttpGet("hashtags")]
        public async Task<IActionResult> GetHashtags([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) 
            {
                return ApiError("關鍵字不能為空");
            }
            var result = await _searchHashtagService.SearchHashtagsAsync(keyword);
            return ApiSuccess(result);
        }
        // GET /api/follows/stats/{personId}
        [HttpGet("stats/{personId:guid}")]
        public async Task<IActionResult> GetStats(Guid personId)
        {
            if (personId == Guid.Empty)
                return BadRequest(new { success = false, message = "personId 無效" });

            var stats = await _followService.GetFollowStatsAsync(personId);
            return Ok(new { success = true, data = stats });
        }

        [HttpGet("tags/{tag}/posts")]
        public async Task<IActionResult> GetPostsByTag(
            [FromRoute] string tag,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var (total, items) = await _searchHashtagService.GetArticlesByTagAsync(tag, page, pageSize);
            return Ok(new { success = true, totalCount = total, articles = items });
        }

    }
}
