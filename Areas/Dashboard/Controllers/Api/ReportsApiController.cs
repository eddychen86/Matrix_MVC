using Matrix.Attributes;
using Matrix.Data;                 // ApplicationDbContext
using Matrix.DTOs;
using Matrix.Extensions;
using Matrix.Services.Interfaces;  // IReportService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;                 // Where/Select/Distinct (視專案需要)

namespace Matrix.Areas.Dashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorization] // 跟頁面一樣，只有管理員可用
    public class Db_ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _db;

        public Db_ReportsController(IReportService reportService, ApplicationDbContext db)
        {
            _reportService = reportService;
            _db = db;
        }

        // GET /api/Db_Reports?page=1&pageSize=20&status=...
        [HttpGet]
        public async Task<IActionResult> List(
            int page = 1, int pageSize = 20,
            int? status = null, int? type = null,
            DateTime? from = null, DateTime? to = null, string? keyword = null)
        {
            try
            {
                Console.WriteLine($"[API] Reports List called with: page={page}, pageSize={pageSize}, status={status}, type={type}, keyword={keyword}");
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
                                    ? (userNames.TryGetValue(r.TargetId, out var name) ? name ?? "-" : "-")
                                    : (articleContents.TryGetValue(r.TargetId, out var content) ? content ?? "-" : "-"),
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
                Console.WriteLine($"[API ERROR] Reports List failed: {ex.Message}");
                Console.WriteLine($"[API ERROR] Stack trace: {ex.StackTrace}");
                return Problem(ex.ToString());
            }
        }

        // 取得當前管理員的 Person 資訊（從 JwtCookieMiddleware 取得 UserId）
        private async Task<(Guid personId, string? displayName)?> GetCurrentAdminPersonAsync()
        {
            Guid userId;

            // 1) 優先：從 JwtCookieMiddleware 設定的 HttpContext.Items 取得
            if (HttpContext.Items["UserId"] is Guid uidFromItems && uidFromItems != Guid.Empty)
            {
                userId = uidFromItems;
            }
            // 2) 退而求其次：從 JWT Claims 的標準 NameIdentifier
            else if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uidFromClaim))
            {
                userId = uidFromClaim;
            }
            // 3) 最後 fallback：從自訂 "UserId" claim
            else if (Guid.TryParse(User.FindFirstValue("UserId"), out var uidFromCustomClaim))
            {
                userId = uidFromCustomClaim;
            }
            else
            {
                return null;
            }

            // 透過 Users.UserId 對應到 Persons.UserId，取得 PersonId 和 DisplayName
            var person = await _db.Persons
                .Where(p => p.UserId == userId)
                .Select(p => new { p.PersonId, p.DisplayName })
                .FirstOrDefaultAsync();

            return person is null ? null : (person.PersonId, person.DisplayName);
        }

        // GET /api/Db_Reports/auth-test - 測試當前認證狀態（暫時不檢查權限）
        [HttpGet("auth-test")]
        [AllowAnonymous]
        public IActionResult TestAuth()
        {
            var authInfo = HttpContext.GetAuthInfo();
            
            return Ok(new
            {
                isAuthenticated = authInfo.IsAuthenticated,
                userId = authInfo.UserId,
                userName = authInfo.UserName,
                displayName = authInfo.DisplayName,
                role = authInfo.Role,
                avatarPath = authInfo.AvatarPath,
                httpContextItems = new
                {
                    userId = HttpContext.Items["UserId"]?.ToString(),
                    isAuthenticated = HttpContext.Items["IsAuthenticated"]?.ToString(),
                    userRole = HttpContext.Items["UserRole"]?.ToString(),
                    displayName = HttpContext.Items["DisplayName"]?.ToString()
                },
                userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                userIdentity = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    name = User.Identity?.Name,
                    authenticationType = User.Identity?.AuthenticationType
                }
            });
        }

        // GET /api/Db_Reports/persons/{personId} - 根據 PersonId 取得人員資訊
        [HttpGet("persons/{personId:guid}")]
        public async Task<IActionResult> GetPersonInfo(Guid personId)
        {
            try
            {
                var person = await _db.Persons
                    .Where(p => p.PersonId == personId)
                    .Select(p => new { p.PersonId, p.DisplayName })
                    .FirstOrDefaultAsync();

                if (person == null)
                    return NotFound();

                return Ok(new
                {
                    personId = person.PersonId,
                    displayName = person.DisplayName,
                    name = person.DisplayName,
                    username = person.DisplayName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API ERROR] GetPersonInfo failed: {ex.Message}");
                return Problem("取得人員資訊時發生錯誤");
            }
        }

        // POST /api/Db_Reports/{id}/process
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

        // POST /api/Db_Reports/{id}/reject
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
