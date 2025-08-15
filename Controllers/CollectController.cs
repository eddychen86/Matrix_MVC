using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
    public class CollectController : Controller
    {
        private readonly ICollectService _collectService;
        private readonly ILogger<CollectController> _logger;

        public CollectController(
            ICollectService collectService,
            ILogger<CollectController> logger)
        {
            _collectService = collectService;
            _logger = logger;
        }

        //GET:Collect
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/collects")]
        public async Task<IActionResult> GetCollectsData()
        {
            try
            {
                // TODO: 未來改為從登入者取得 PersonId，例如 User.Identity.Name → User → Person.Id
                //var currentUserId = Guid.Parse("36a9c596-b298-49b5-8300-7c3479aed145");
                // ✅ 從登入者取得 PersonId（專案已有擴充方法）
                var authInfo = HttpContext.GetAuthInfo();

                if (authInfo == null || authInfo.PersonId == Guid.Empty)
                {
                    return Unauthorized(new { error = "未登入或無效的使用者資料" });
                }

                var personId = authInfo.PersonId;

                var collectDtos = await _collectService.GetUserCollectsAsync(personId, 30);
                
                var collectViewModels = collectDtos.Select(dto => new CollectItemViewModel
                {
                    Title = dto.Title,
                    ImageUrl = dto.ImageUrl,
                    AuthorName = dto.AuthorName,
                    CollectedAt = dto.CollectedAt
                }).ToList();

                return Json(collectViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collects data");
                return Json(new { error = "取得收藏資料時發生錯誤" });
            }
        }
    }
}
