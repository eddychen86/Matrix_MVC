using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class SearchController : Controller
    {
        public readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("/api/search")]
        public async Task<IActionResult> GetSearchResults([FromQuery] string keyword) 
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<object>());
            keyword = keyword.Trim().ToLower();

            // 1️⃣ 撈使用者（從 Persons 表）
            var userMatches = await _context.Persons
                .Where(p => p.DisplayName.ToLower().Contains(keyword))
                .Select(p => new
                {
                    type = "user",
                    id = p.PersonId,
                    displayName = p.DisplayName,
                    bio = p.Bio,
                    avatar = $"/static/avatar/{p.PersonId}.png"  // 根據你們的專案路徑調整
                })
                .ToListAsync();

            // 2️⃣ 撈標籤（從 Hashtags 表）
            var tagMatches = await _context.Hashtags
                .Where(h => h.Content.ToLower().Contains(keyword))
                .Select(h => new
                {
                    type = "tag",
                    id = h.TagId,
                    content = h.Content
                })
                .ToListAsync();

            // 合併回傳
            var result = userMatches.Concat<object>(tagMatches).ToList();

            return Ok(result);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
