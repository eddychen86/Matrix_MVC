using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
    public class NotifyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotifyController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //GET:Notify
        [HttpGet]
        [Route("/api/Notify")]
        public IActionResult GetNotifyData()
        {
            // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("36a9c596-b298-49b5-8300-7c3479aed145");

            var notifications = _context.Notifications
                .AsNoTracking() // 只讀查詢，優化效能
                .Where(n => n.GetId == currentUserId)
                .Include(n => n.Sender) // 載入 Sender 的 Person 對象
                .OrderByDescending(n => n.SentTime)
                .Take(10)
                .ToList();

            var notifyList = notifications.Select(n => new NotifyItemViewModel
            {
                SenderName = n.Sender.DisplayName ?? "匿名",
                SenderAvatarUrl = !string.IsNullOrEmpty(n.Sender.AvatarPath) ? n.Sender.AvatarPath : "/static/img/cute.png",
                Message = n.Type == 0 ? "有人留言了你的貼文" : n.Type == 1 ? "有人私訊你" : "有新通知",
                SentTime = n.SentTime
            }).ToList();

            return Json(notifyList);
        }
    }
}
