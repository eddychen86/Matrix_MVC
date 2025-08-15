using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/follows")]
    public class FollowsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFollowService _followService;

        public FollowsController(ApplicationDbContext context, IFollowService followService)
        {
            _context = context;
            _followService = followService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowData(int page = 1, int pageSize = 10)
        {
            var auth = HttpContext.GetAuthInfo();
            Guid currentUserId = auth?.PersonId ?? Guid.Empty;

            if (currentUserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var followList = await _context.Follows
                .AsNoTracking()
                .Where(f => f.UserId == currentUserId && f.Type == 1)
                .OrderByDescending(f => f.FollowTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.Persons.AsNoTracking(),
                    f => f.FollowedId,
                    p => p.PersonId,
                    (f, p) => new FollowItemViewModel
                    {
                        PersonId = p.PersonId,
                        SenderName = string.IsNullOrWhiteSpace(p.DisplayName) ? "未知用戶" : p.DisplayName,
                        SenderAvatarUrl = string.IsNullOrEmpty(p.AvatarPath)
                            ? "/static/img/cute.png"
                            : p.AvatarPath,
                        FollowTime = f.FollowTime
                    })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = followList,
                pagination = new { page, pageSize }
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || auth.PersonId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            var result = await _followService.SearchUsersAsync(auth.PersonId, keyword);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("{targetId}")]
        public async Task<IActionResult> FollowUser(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || auth.PersonId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            var success = await _followService.FollowAsync(auth.PersonId, targetId);
            return Ok(new { success });
        }

        [HttpDelete("{targetId}")]
        public async Task<IActionResult> UnfollowUser(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || auth.PersonId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            var success = await _followService.UnfollowAsync(auth.PersonId, targetId);
            return Ok(new { success });
        }

        [HttpGet("is-following")]
        public async Task<IActionResult> IsFollowing(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || auth.PersonId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            var isFollow = await _followService.IsFollowingAsync(auth.PersonId, targetId);
            return Ok(new { success = true, isFollow });
        }
    }
}
