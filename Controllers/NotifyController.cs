using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers
{
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
        [Route("/api/notify")]
        public IActionResult GetNotifyData()
        {
            // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("7ffbe594-de54-452a-92b0-311631587369");

            var notifications = _context.Notifications
                .Where(n => n.GetId == currentUserId)
                .Include(n => n.Sender) // 載入 Sender 的 Person 對象
                .OrderByDescending(n => n.SentTime)
                .Take(10)
                .ToList();


            var notifyList = notifications.Select(n => new NotifyItemViewModel
            { 
                SenderName = n.Sender.DisplayName ?? "匿名",
                SenderAvatarUrl = n.Sender.AvatarPath ?? "/static/img/default-avatar.png",
                Message = n.Type == 0 ?"有人留言了你的貼文"
                          : n.Type == 1 ? "有人私訊你"
                          :"有新通知",
                SentTime = n.SentTime
            }).ToList();

            return Json(notifyList);

        }
    }
}
