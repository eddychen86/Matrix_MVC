using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Matrix.Data;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using Matrix.Extensions;
using Matrix.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/follows")]
    public class FollowsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFollowService _followService;
        private readonly IPersonRepository _personRepository;
        private readonly INotificationService _notificationService;

        public FollowsController(ApplicationDbContext context, IFollowService followService, IPersonRepository personRepository, INotificationService notificationService)
        {
            _context = context;
            _followService = followService;
            _personRepository = personRepository;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowData(int page = 1, int pageSize = 10)
        {
            var auth = HttpContext.GetAuthInfo();
            
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var currentUserId = person.PersonId;

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
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var result = await _followService.SearchUsersAsync(person.PersonId, keyword);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("{targetId}")]
        public async Task<IActionResult> FollowUser(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var success = await _followService.FollowAsync(person.PersonId, targetId);


            if (success)
            {
                // 🔔 送「追蹤你」通知：這裡要用 UserId
                // targetId 是對方的 PersonId，先找出對方的 UserId
                var targetPerson = await _context.Persons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonId == targetId);

                if (targetPerson?.UserId != null && targetPerson.UserId != Guid.Empty)
                {
                    // type=3 代表「追蹤」
                    await _notificationService.SendUserNotificationAsync(
                        senderId: auth.UserId,            // 我（UserId）
                        receiverId: targetPerson.UserId,  // 對方（UserId）
                        type: 3
                    );
                }
            }
            return Ok(new { success });
        }

        [HttpDelete("{targetId}")]
        public async Task<IActionResult> UnfollowUser(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var success = await _followService.UnfollowAsync(person.PersonId, targetId);
            return Ok(new { success });
        }

        [HttpGet("is-following")]
        public async Task<IActionResult> IsFollowing(Guid targetId)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var isFollow = await _followService.IsFollowingAsync(person.PersonId, targetId);
            return Ok(new { success = true, isFollow });
        }
    }
}
