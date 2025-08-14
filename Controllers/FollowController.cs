using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers
{
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFollowService _followService;

        public FollowController(ApplicationDbContext context, IFollowService followService)
        {
            _context = context;
            _followService = followService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/follows")]
        public async Task<IActionResult> GetFollowData(int page = 1, int pageSize = 10)
        {
            // 1) 取得目前登入者的 UserId
            Guid currentUserId;
            var auth = HttpContext.GetAuthInfo(); // 若你們專案有這個擴充方法
            if (auth != null && auth.UserId != Guid.Empty)
            {
                currentUserId = auth.PersonId;
            }
            else
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out currentUserId))
                    return Unauthorized(new { success = false, message = "尚未登入" });
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;


            // 2) 只撈「使用者」型別的追蹤 (Type = 1)，並 JOIN 到 Persons（FollowedId = PersonId）
            var followList = await _context.Follows
                .AsNoTracking()
                .Where(f => f.UserId == currentUserId && f.Type == 1)
                .OrderByDescending(f => f.FollowTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.Persons.AsNoTracking(),
                    f => f.FollowedId,           // ← FollowedId = Persons.PersonId（你選的路線A）
                    p => p.PersonId,
                    (f, p) => new FollowItemViewModel
                    {
                        SenderName = string.IsNullOrWhiteSpace(p.DisplayName) ? "未知用戶" : p.DisplayName,
                        SenderAvatarUrl = string.IsNullOrEmpty(p.AvatarPath)
                            ? "/static/img/cute.png" // 改成你的預設圖路徑
                            : p.AvatarPath,
                        FollowTime = f.FollowTime
                    }
                )
                .ToListAsync();

            // 3) 建議用 Ok(...)（標準 API 回應）
            return Ok(new
            {
                success = true,
                data = followList,
                pagination = new { page, pageSize }
            });
        }
        [HttpGet("search")]
        [Route("api/follows/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
        {
            var auth = HttpContext.GetAuthInfo();
            if (auth == null || auth.PersonId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            var currentPersonId = auth.PersonId;

            var result = await _followService.SearchUsersAsync(currentPersonId, keyword);

            return Ok(new { success = true, data = result });
        }
    }
}