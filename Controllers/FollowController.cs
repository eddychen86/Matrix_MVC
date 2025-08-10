using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers
{
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FollowController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/follows")]
        public IActionResult GetFollowData()
        {
            // TODO: 未來改成從登入者取得 PersonId
            var currentUserId = Guid.Parse("ba19a9e2-53ab-4764-9cd4-04a9d6595515"); 

            var follows = _context.Follows
                .AsNoTracking() // 只讀查詢，優化效能
                .Where(f => f.UserId == currentUserId)
                .Include(f => f.User) // 載入 User 的 Person 對象
                .OrderByDescending(f => f.FollowTime)
                .Take(10)
                .ToList();

            var followedIds = follows.Select(f=>f.FollowedId).ToList();

            var followedPeople = _context.Persons
                .AsNoTracking() // 只讀查詢
                .Where(p => followedIds.Contains(p.PersonId))
                .ToDictionary(p => p.PersonId, p => p);

            var followList = follows.Select(f =>
            {
                followedPeople.TryGetValue(f.FollowedId, out var person);

                return new FollowItemViewModel
                {
                    SenderName = person?.DisplayName ?? "未知用戶",
                    SenderAvatarUrl = !string.IsNullOrEmpty(person?.AvatarPath) ? person.AvatarPath : "/static/img/cute.png",
                    FollowTime = f.FollowTime
                };
            }).ToList();

            return Json(followList);



        }
        

    }
}
