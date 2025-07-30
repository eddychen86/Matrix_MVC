using Microsoft.AspNetCore.Mvc;
using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers
{
    public class CollectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollectController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // API endpoint for popup data
        [HttpGet]
        [Route("/api/Collects")]
        public IActionResult GetCollectsData()
        {
            // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
            var currentUserId = Guid.Parse("36a9c596-b298-49b5-8300-7c3479aed145");

            var collects = _context.PraiseCollects
                .Include(i => i.Article)
                .Include(i => i.User)
                .Where(p => p.Type == 1 && p.UserId == currentUserId)
                .OrderByDescending(p => p.CreateTime)
                .Take(30)
                .ToList()
                .Select(p => new CollectItemViewModel
                {
                    Title = p.Article?.Content?.Substring(0, Math.Min(10, p.Article.Content?.Length ?? 0)) ?? "",
                    ImageUrl = "https://i.imgur.com/YVy64ke.jpg",
                    AuthorName = p.User?.DisplayName ?? "匿名",
                    CollectedAt = p.CreateTime
                })
                .ToList();

            return Json(collects);
        }
    }
}
