using Matrix.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Matrix.Data;                 // ApplicationDbContext
using Matrix.Services.Interfaces;  // IReportService
using System.Linq;                 // Where/Select/Distinct (視專案需要)

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

                var userNames = await _db.Persons
                    .Where(p => userIds.Contains(p.PersonId))
                    .Select(p => new { p.PersonId, p.DisplayName })
                    .ToDictionaryAsync(x => x.PersonId, x => x.DisplayName);


                var articleContents = await _db.Articles
                    .Where(a => articleIds.Contains(a.ArticleId))
                    .Select(a => new { a.ArticleId, a.Content })
                    .ToDictionaryAsync(x => x.ArticleId, x => x.Content);

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
                    Status = r.Status switch { 0 => "Pending", 1 => "Processed", 2 => "Rejected", _ => r.Status.ToString() }
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

        // POST /api/dashboard/reports/{id}/process
        [HttpPost("{id:guid}/process")]
        public async Task<IActionResult> Process(Guid id)
        {
            var ok = await _reportService.ProcessReportAsync(id, Guid.Empty /* TODO: 換成實際 AdminId */);
            return ok ? Ok() : NotFound();
        }

        // POST /api/dashboard/reports/{id}/reject
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var ok = await _reportService.RejectReportAsync(id, Guid.Empty /* TODO: 換成實際 AdminId */);
            return ok ? Ok() : NotFound();
        }



        // DTO 與分頁模型（你也可以抽到共用檔案）
        public class ReportListItemDto
        {
            public Guid ReportId { get; set; }
            public string Reason { get; set; } = "";
            public string Reporter { get; set; } = "";
            public string Type { get; set; } = "";
            public string Target { get; set; } = "";
            public DateTime? CreateTime { get; set; }
            public DateTime? ModifyTime { get; set; }
            public string Status { get; set; } = "";
        }
        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
}
