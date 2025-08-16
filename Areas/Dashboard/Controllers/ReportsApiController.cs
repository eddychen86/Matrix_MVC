using Matrix.Attributes;
using Matrix.Data;                 // ApplicationDbContext
using Matrix.DTOs;
using Matrix.Services.Interfaces;  // IReportService
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;                 // Where/Select/Distinct (視專案需要)

namespace Matrix.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [ApiController]
    [AdminAuthorization] // 跟頁面一樣，只有管理員可用
    [Route("api/dashboard/reports")]
    public class Db_ReportsApiController : ControllerBase
   {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _db;

        public Db_ReportsApiController(IReportService reportService, ApplicationDbContext db)
        {
            _reportService = reportService;
            _db = db;
        }

        // GET /api/dashboard/reports?page=1&pageSize=20&status=...
        [HttpGet]
        public async Task<IActionResult> List(
            int page = 1, int pageSize = 20,
            int? status = null, int? type = null,
            DateTime? from = null, DateTime? to = null, string? keyword = null)
        {
            try
            {
                // ✅ 把所有條件傳給 Service（新多載）
                var (reports, total) = await _reportService.GetReportsAsync(
                    page: page,
                    pageSize: pageSize,
                    status: status,
                    type: type,
                    keyword: keyword,
                    from: from,
                    to: to,
                    reporterId: null,
                    reportedUserId: null
                );

                // 批次查 Target 顯示字串（避免 N+1）
                var userIds = reports.Where(r => r.Type == 0).Select(r => r.TargetId).Distinct().ToList();
                var articleIds = reports.Where(r => r.Type == 1).Select(r => r.TargetId).Distinct().ToList();
                // ▶ 取得所有已經有處理者的 ResolverId（PersonId）
                var resolverIds = reports
                    .Where(r => r.ResolverId.HasValue)
                    .Select(r => r.ResolverId!.Value)
                    .Distinct()
                    .ToList();

                var userNames = await _db.Persons
                    .Where(p => userIds.Contains(p.PersonId))
                    .Select(p => new { p.PersonId, p.DisplayName })
                    .ToDictionaryAsync(x => x.PersonId, x => x.DisplayName);


                var articleContents = await _db.Articles
                    .Where(a => articleIds.Contains(a.ArticleId))
                    .Select(a => new { a.ArticleId, a.Content })
                    .ToDictionaryAsync(x => x.ArticleId, x => x.Content);

                var resolverNames = await _db.Persons                                     // ★
                    .Where(p => resolverIds.Contains(p.PersonId))
                    .Select(p => new { p.PersonId, p.DisplayName })
                    .ToDictionaryAsync(x => x.PersonId, x => x.DisplayName);

                // 映射成前端需要的欄位（CreateTime 先回 null；ModifyTime 用 ProcessTime）
                var items = reports.Select(r => new ReportListItemDto
                {
                    ReportId = r.ReportId,
                    Reason = r.Reason,
                    Reporter = r.Reporter?.DisplayName ?? "(unknown)",
                    Type = r.Type == 0 ? "User" : "Article",
                    Target = r.Type == 0
                                    ? (userNames.TryGetValue(r.TargetId, out var name) ? name : "-")
                                    : (articleContents.TryGetValue(r.TargetId, out var content) ? content : "-"),
                    CreateTime = null,              // 沒有欄位 → 回 null，前端會顯示 "-"
                    ModifyTime = r.ProcessTime,     // 用處理/駁回時間當 ModifyTime


                    Status = r.Status,

                    ResolverId = r.ResolverId,
                    ResolverName = (r.ResolverId.HasValue && resolverNames.TryGetValue(r.ResolverId.Value, out var rn))
                            ? rn : null
                }).ToList();

                return Ok(new PagedResult<ReportListItemDto>
                {
                    Items = items,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
        private async Task<(Guid personId, string? displayName)?> GetCurrentAdminPersonAsync()
        {
            // 從登入票找 ASP.NET Identity 的 UserId
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return null;

            // 用 Users.UserId 對應到 Persons.UserId，取出 PersonId（就是要寫進 Reports.ResolverId 的值）
            var p = await _db.Persons
                .Where(x => x.UserId == userId)
                .Select(x => new { x.PersonId, x.DisplayName })
                .FirstOrDefaultAsync();

            return p is null ? null : (p.PersonId, p.DisplayName);
        }

        // POST /api/dashboard/reports/{id}/process
        [HttpPost("{id:guid}/process")]
        public async Task<IActionResult> Process(Guid id)
        {
            Console.WriteLine("IsAuthenticated: " + User.Identity?.IsAuthenticated);
            foreach (var c in User.Claims)
                Console.WriteLine($"{c.Type} = {c.Value}");


            var me = await GetCurrentAdminPersonAsync();
            if (me is null) return Unauthorized();

            var (adminPid, adminName) = me.Value;

            var ok = await _reportService.ProcessReportAsync(id, adminPid); // ← 傳 PersonId
            if (!ok) return NotFound();

            return Ok(new
            {
                resolverId = adminPid,
                resolverName = adminName,
                processTime = DateTime.UtcNow
            });
        }
        // POST /api/dashboard/reports/{id}/reject
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var me = await GetCurrentAdminPersonAsync();
            if (me is null) return Unauthorized();

            var (adminPid, adminName) = me.Value;

            var ok = await _reportService.RejectReportAsync(id, adminPid);
            if (!ok) return NotFound();

            return Ok(new
            {
                resolverId = adminPid,
                resolverName = adminName,
                processTime = DateTime.UtcNow
            });
        }



        // DTO 與分頁模型（你也可以抽到共用檔案）

        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
}
