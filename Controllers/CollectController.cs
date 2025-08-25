using Microsoft.AspNetCore.Mvc;
using Matrix.Attributes;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using Matrix.Extensions;
using Matrix.ViewModels;

namespace Matrix.Controllers
{
    [MemberAuthorization] // 需要一般會員權限 (Role >= 0)
    public class CollectController : Controller
    {
        private readonly ICollectService _collectService;
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<CollectController> _logger;

        public CollectController(
            ICollectService collectService,
            IPersonRepository personRepository,
            ILogger<CollectController> logger)
        {
            _collectService = collectService;
            _personRepository = personRepository;
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
                // 從認證資訊取得 UserId
                var authInfo = HttpContext.GetAuthInfo();

                if (authInfo == null || !authInfo.IsAuthenticated || authInfo.UserId == Guid.Empty)
                {
                    return Unauthorized(new { error = "未登入或無效的使用者資料" });
                }

                // 透過 UserId 查詢對應的 PersonId
                var person = await _personRepository.GetByUserIdAsync(authInfo.UserId);
                if (person == null)
                {
                    return Unauthorized(new { error = "找不到對應的用戶資料" });
                }

                var personId = person.PersonId;

                var collectDtos = await _collectService.GetUserCollectsAsync(personId, 30);
                
                var collectViewModels = collectDtos.Select(dto => new CollectItemViewModel
                {
                    ArticleId = dto.ArticleId,
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
