using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")] // /api/reports
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _conf;

        public ReportsController(IReportService reportService, ApplicationDbContext db, IConfiguration conf)
        {
            _reportService = reportService;
            _db = db;
            _conf = conf;
        }

        public class CreateReportRequest
        {
            /// <summary>0=User, 1=Article</summary>
            public int Type { get; set; }
            /// <summary>Type=0 傳被檢舉 PersonId；Type=1 傳 ArticleId</summary>
            public Guid TargetId { get; set; }
            /// <summary>Type=1（檢舉文章）需帶作者 PersonId（因為 Service 需要）</summary>
            public Guid? ReportedUserId { get; set; }
            public required string Reason { get; set; }
            public string? Description { get; set; }
        }

        // ✅ 嘗試從多種來源解析「目前登入者」
        private async Task<(bool ok, Guid personId, Guid userId)> TryResolveAuthAsync()
        {
            // 1) 先走你們既有的擴充（多半吃 Cookie/Middleware）
            var auth = HttpContext.GetAuthInfo();
            if (auth?.IsAuthenticated == true && auth.PersonId != Guid.Empty)
                    return (true, auth.PersonId, auth.UserId);

            // 2) 退而求其次：解析 Authorization: Bearer <token>
            var header = Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header.Substring("Bearer ".Length).Trim();
                var principal = ValidateJwt(token);
                if (principal != null)
                {
                    // 取 UserId（NameIdentifier 或自訂 UserId）
                    var uidStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? principal.FindFirst("UserId")?.Value;
                    if (Guid.TryParse(uidStr, out var userId))
                    {
                        // 對應 Persons 拿 PersonId
                        var person = await _db.Persons
                            .AsNoTracking()
                            .Where(p => p.UserId == userId)
                            .Select(p => new { p.PersonId })
                            .FirstOrDefaultAsync();

                        if (person != null)
                            return (true, person.PersonId, userId);
                    }
                }
            }

            // 3) 最後試試 Cookie（若有不同於 middleware 的情況）
            var cookieToken = Request.Cookies["AuthToken"];
            if (!string.IsNullOrWhiteSpace(cookieToken))
            {
                var principal = ValidateJwt(cookieToken);
                if (principal != null)
                {
                    var uidStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? principal.FindFirst("UserId")?.Value;
                    if (Guid.TryParse(uidStr, out var userId))
                    {
                        var person = await _db.Persons
                            .AsNoTracking()
                            .Where(p => p.UserId == userId)
                            .Select(p => new { p.PersonId })
                            .FirstOrDefaultAsync();

                        if (person != null)
                            return (true, person.PersonId, userId);
                    }
                }
            }

            return (false, Guid.Empty, Guid.Empty);
        }

        // 只給本 Controller 用的小工具：用與專案相同的 Key/Issuer 驗證 JWT
        private ClaimsPrincipal? ValidateJwt(string token)
        {
            try
            {
                var key = _conf["JWT:Key"];
                var issuer = _conf["JWT:Issuer"];
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(issuer)) return null;

                var handler = new JwtSecurityTokenHandler();
                var parms = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                var principal = handler.ValidateToken(token, parms, out var validated);
                if (validated is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return principal;

                return null;
            }
            catch { return null; }
        }

        [HttpPost] // 不掛 [Authorize]，我們自己判斷登入
        public async Task<IActionResult> Create([FromBody] CreateReportRequest req)
        {
            // 🟢 這裡改用 TryResolveAuth，不動你們的登入/驗證設定
            var me = await TryResolveAuthAsync();
            if (!me.ok) return Unauthorized(new { success = false, message = "尚未登入" });

            if (req is null || (req.Type != 0 && req.Type != 1))
                return BadRequest(new { success = false, message = "檢舉類型不正確" });

            var reason = (req.Reason ?? "").Trim();
            if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
                return BadRequest(new { success = false, message = "請輸入 1~500 字的檢舉理由" });

            // 目標存在性
            if (req.Type == 0)
            {
                var exists = await _db.Persons.AsNoTracking().AnyAsync(p => p.PersonId == req.TargetId);
                if (!exists) return NotFound(new { success = false, message = "找不到被檢舉的使用者" });
            }
            else // Type == 1 檢舉文章
            {
                // 先確認文章存在並取出作者 PersonId
                var article = await _db.Articles
                    .AsNoTracking()
                    .Where(a => a.ArticleId == req.TargetId)
                    .Select(a => new
                    {
                        a.ArticleId,
                        // 🔽 下面這個欄位請改成你文章表中「作者的 PersonId」欄位名稱
                        AuthorPersonId = a.AuthorId   // ← 若你的欄位不是 AuthorId，改成 PersonId/CreatorId/OwnerId 等實際名稱
                    })
                    .FirstOrDefaultAsync();

                if (article == null)
                    return NotFound(new { success = false, message = "找不到被檢舉的文章" });

                // 前端若沒帶作者，就用資料庫查到的作者補上
                if (!req.ReportedUserId.HasValue || req.ReportedUserId.Value == Guid.Empty)
                    req.ReportedUserId = article.AuthorPersonId;

                if (!req.ReportedUserId.HasValue || req.ReportedUserId.Value == Guid.Empty)
                    return BadRequest(new { success = false, message = "缺少文章作者 Id" });
            }

            // 防重複（未處理中的同人同標的）
            var dup = await _db.Reports.AsNoTracking().AnyAsync(r =>
                r.ReporterId == me.personId &&
                r.TargetId == (req.Type == 1 ? req.TargetId : req.TargetId) &&
                r.Type == req.Type &&
                r.Status == 0 &&
                r.ProcessTime == null
            );
            if (dup)
                return Ok(new { success = true, message = "你最近已檢舉過此對象，我們會儘速處理。" });

            // 呼叫你既有的 Service 簽名
            var ok = await _reportService.CreateReportAsync(
                reporterId: me.personId,
                reportedUserId: req.Type == 0 ? req.TargetId : req.ReportedUserId!.Value,
                articleId: req.Type == 1 ? req.TargetId : (Guid?)null,
                reason: reason,
                description: req.Description
            );

            if (!ok) return Problem("建立檢舉失敗");
            return Ok(new { success = true });
        }
    }
}
