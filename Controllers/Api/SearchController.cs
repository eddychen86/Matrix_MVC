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
        public FollowsController(IFollowService followService) => _followService = followService;
        public SearchController(ISearchUserService searchUserService, ISearchHashtagService searchHashtagService)
        {
            _searchUserService = searchUserService;
            _searchHashtagService = searchHashtagService;
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
    }
}
