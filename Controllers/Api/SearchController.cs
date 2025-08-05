using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchUserService _searchUserService;

        public SearchController(ISearchUserService searchUserService)
        {
            _searchUserService = searchUserService;
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
    }
}
