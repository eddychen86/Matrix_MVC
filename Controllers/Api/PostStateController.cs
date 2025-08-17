using Humanizer;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Matrix.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostStateController : ControllerBase
    {
        private readonly IPraiseService _praiseService;
        private readonly IPraiseCollectRepository _praiseCollectRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IArticleRepository _articleRepository;

        public PostStateController(
            IPraiseService praiseService,
            IPraiseCollectRepository praiseCollectRepository,
            IPersonRepository personRepository,
            IArticleRepository articleRepository)
        {
            _praiseService = praiseService;
            _praiseCollectRepository = praiseCollectRepository;
            _personRepository = personRepository;
            _articleRepository = articleRepository;
        }


        // 兼容常見的 JWT Claim：NameIdentifier / sub
        Guid? CurrentUserId()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(s, out var id) ? id : (Guid?)null;
        }

        public class ToggleStateDto { public Guid ArticleId { get; set; } }

        [HttpPost("praise")]
        public async Task<IActionResult> Praise([FromBody] ToggleStateDto dto)
        {
            try
            {
                if (dto == null || dto.ArticleId == Guid.Empty)
                {
                    return BadRequest(new { message = "Invalid articleId." });
                }

                var authUserId = CurrentUserId();
                if (authUserId is null)
                {
                    return Unauthorized(new { message = "Unauthorized" });
                }

                // 把 UserId 轉成 PersonId（PraiseCollect 外鍵是 Persons.PersonId）
                var person = await _personRepository.GetByUserIdAsync(authUserId.Value);
                if (person == null)
                {
                    return BadRequest(new { message = "Person profile not found for current user." });
                }
                ;

                var personId = person.PersonId;

                // 傳 PersonId 下去
                var result = await _praiseService.TogglePraiseAsync(dto.ArticleId, personId);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                var article = await _articleRepository.GetByIdAsync(dto.ArticleId);
                return Ok(new { success = true, isPraised = result.Data, praiseCount = article?.PraiseCount ?? 0 });
            }
            catch (Exception ex)
            {
                Console.WriteLine("PostState/Praise error: " + ex);
                return StatusCode(500, new { message = "Server error", stack = ex.ToString() });
            }
        }

        [HttpPost("collect")]
        public async Task<IActionResult> Collect([FromBody] ToggleStateDto dto)
        {
            try
            {
                if (dto == null || dto.ArticleId == Guid.Empty)
                {
                    return BadRequest(new { message = "Invalid articleId." });
                }
                ;


                var authUserId = CurrentUserId();
                if (authUserId is null)
                {
                    return Unauthorized(new { message = "Unauthorized" });
                }
                ;

                // UserId → PersonId
                var person = await _personRepository.GetByUserIdAsync(authUserId.Value);
                if (person == null)
                {
                    return BadRequest(new { message = "Person profile not found for current user." });
                }
                ;
                var personId = person.PersonId;

                // 只取一次文章做存在檢查 + 後面讀計數
                var article = await _articleRepository.GetByIdAsync(dto.ArticleId);
                if (article == null)
                {
                    return BadRequest(new { message = "Article not found." });
                }
                ;

                var had = await _praiseCollectRepository.HasUserCollectedAsync(personId, dto.ArticleId);
                await _praiseCollectRepository.UpdateCollectStatusAsync(personId, dto.ArticleId, !had);

                if (had)
                {
                    await _articleRepository.DecreaseCollectCountAsync(dto.ArticleId);
                }
                else
                {
                    await _articleRepository.IncreaseCollectCountAsync(dto.ArticleId);
                }


                return Ok(new { success = true, isCollected = !had, collectCount = article.CollectCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine("PostState/Collect error: " + ex);
                return StatusCode(500, new { message = "Server error", stack = ex.ToString() });
            }
        }

        private async Task<Guid?> GetCurrentPersonId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(idStr, out var userId))
            {
                return null;
            }

            var person = await _personRepository.GetByUserIdAsync(userId);
            return person?.PersonId;
        }

        [HttpGet("feed")]
        public async Task<IActionResult> Feed(int page = 1, int pageSize = 20)
        {
            var personId = await GetCurrentPersonId();
            var (items, total) = await _articleRepository.GetArticlesWithUserStateAsync(page, pageSize, personId);
            return Ok(new { items, total, page, pageSize });
        }

    }
}
