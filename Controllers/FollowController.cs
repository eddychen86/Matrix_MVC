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
            var currentUserId = Guid.Parse("7ffbe594-de54-452a-92b0-311631587369"); 

            var follows = _context.Follows
                .Where(f => f.UserId == currentUserId)
                .Include(f => f.User) // 載入 User 的 Person 對象
                .OrderByDescending(f => f.FollowTime)
                .Take(10)
                .ToList();

            var followedIds = follows.Select(f=>f.FollowedId).ToList();

            var followedPeople = _context.Persons
                .Where(p => followedIds.Contains(p.PersonId))
                .ToDictionary(p => p.PersonId, p => p);

            var followList = follows.Select(f =>
            {
                followedPeople.TryGetValue(f.FollowedId, out var person);

                return new FollowItemViewModel
                {
                    SenderName = person?.DisplayName ?? "未知用戶",
                    SenderAvatarUrl = (person?.AvatarPath != null && person.AvatarPath.Length > 0) ? $"data:image/jpeg;base64,{Convert.ToBase64String(person.AvatarPath)}" : "",
                    FollowTime = f.FollowTime
                };
            }).ToList();

            return Json(followList);



        }
        //public IActionResult TestPerson()
        //{
        //    // 先檢查是否已有 user123
        //    var existingUser = _context.Users.Find("user123");
        //    var existingPerson = _context.Persons.Find("user123");

        //    if (existingUser != null || existingPerson != null)
        //    {
        //        return Content("⚠ user123 已經存在，無需再次新增！");
        //    }

        //    // 建立 IdentityUser
        //    var identityUser = new IdentityUser
        //    {
        //        Id = "user123", // 跟 PersonId 對應
        //        UserName = "testuser",
        //        Email = "test@example.com",
        //        NormalizedUserName = "TESTUSER",
        //        NormalizedEmail = "TEST@EXAMPLE.COM",
        //        EmailConfirmed = true,
        //        SecurityStamp = Guid.NewGuid().ToString()
        //    };

        //    _context.Users.Add(identityUser);

        //    // 建立 Person 並綁定 User 導航屬性
        //    var person = new Person
        //    {
        //        PersonId = "user123",
        //        DisplayName = "測試用名稱",
        //        AvatarPath = "https://randomuser.me/api/portraits/women/1.jpg",
        //        Bio = "這是測試用帳號",
        //        WalletAddress = "0x1234567890abcdef",
        //        ModifyTime = DateTime.Now,
        //        User = identityUser
        //    };

        //    _context.Persons.Add(person);
        //    _context.SaveChanges();

        //    return Content("✅ 測試用 Person 已成功新增！");
        //}
        //public IActionResult TestFollow()
        //{
        //    var testPerson = _context.Persons.FirstOrDefault(p => p.PersonId == "user123");

        //    if (testPerson == null)
        //    {
        //        return Content("⚠ 請先執行 TestPerson，建立 user123 帳號");
        //    }

        //    var follow = new Follow
        //    {
        //        FollowId = Guid.NewGuid(),
        //        UserId = testPerson.PersonId,
        //        IsFollowingId = Guid.NewGuid(), // 假追蹤對象
        //        Type = 1,
        //        FollowTime = DateTime.Now,
        //        User = testPerson // 記得帶入導航屬性
        //    };

        //    _context.Follows.Add(follow);
        //    _context.SaveChanges();

        //    return Content("✅ 成功新增一筆 Follow 資料！");
        //}


    }
}
