using Matrix.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/notify")]
    [MemberAuthorization] // 需要一般會員(Role >= 0)，未登入會擋掉
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPersonRepository _personRepository;

        public NotificationsController(ApplicationDbContext context, IPersonRepository personRepository)
        {
            _context = context;
            _personRepository = personRepository;
        }

        /// <summary>
        /// 取得「目前登入者」的最新通知（預設 10 筆）
        /// GET /api/notify?take=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int take = 10)
        {
            var auth = HttpContext.GetAuthInfo();
            
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var currentUserId = person.PersonId;

            if (take <= 0) take = 10;
            if (take > 50) take = 50; // 簡單限制，避免一次抓太多

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.GetId == currentUserId)
                .Include(n => n.Sender)  // 載入發送者 Person
                .OrderByDescending(n => n.SentTime)
                .Take(take)
                .ToListAsync();

            var list = notifications.Select(n => new NotifyItemViewModel
            {
                SenderName = n.Sender?.DisplayName ?? "匿名",
                SenderAvatarUrl = !string.IsNullOrWhiteSpace(n.Sender?.AvatarPath)
                                    ? n.Sender!.AvatarPath!
                                    : "/static/img/cute.png",
                Message = n.Type == 0 ? "有人留言了你的貼文"
                                  : n.Type == 1 ? "有人私訊你"
                                  : "有新通知",
                SentTime = n.SentTime,
                IsRead = n.IsRead == 1,
                NotifyId = n.NotifyId
            }).ToList();

            return Ok(new { success = true, data = list });
        }

        /// <summary>
        /// 將某筆通知標記為已讀
        /// PATCH /api/notify/{notifyId}/read
        /// </summary>
        [HttpPatch("{notifyId:guid}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid notifyId)
        {
            var auth = HttpContext.GetAuthInfo();
            
            if (auth == null || !auth.IsAuthenticated || auth.UserId == Guid.Empty)
                return Unauthorized(new { success = false, message = "尚未登入" });

            // 透過 UserId 查詢對應的 PersonId
            var person = await _personRepository.GetByUserIdAsync(auth.UserId);
            if (person == null)
                return Unauthorized(new { success = false, message = "找不到對應的用戶資料" });

            var currentUserId = person.PersonId;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotifyId == notifyId && n.GetId == currentUserId);

            if (notification == null)
                return NotFound(new { success = false, message = "通知不存在" });

            notification.IsRead = 1;
            notification.IsReadTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
    public class NotifyItemViewModel
    {
        public Guid NotifyId { get; set; }
        public string SenderName { get; set; } = "";
        public string SenderAvatarUrl { get; set; } = "/static/img/cute.png";
        public string Message { get; set; } = "有新通知";
        public DateTime SentTime { get; set; }
        public bool IsRead { get; set; }
    }
}

