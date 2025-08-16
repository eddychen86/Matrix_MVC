using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public PostStateController(IPraiseService praiseService, IPraiseCollectRepository praiseCollectRepository)
        {
            _praiseService = praiseService;
            _praiseCollectRepository = praiseCollectRepository;
        }

        Guid? CurrentUserId()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var id) ? id : null;
        }
        public class ToggleStateDto { public Guid ArticleId { get; set; } }


        [HttpPost("praise")]
        public async Task<IActionResult> Praise([FromBody] ToggleStateDto dto)
        {
            if (dto == null || dto.ArticleId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid articleId." });
            }

            var uid = CurrentUserId();
            if (uid is null)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var result = await _praiseService.TogglePraiseAsync(dto.ArticleId, uid.Value);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            var count = await _praiseCollectRepository.CountPraisesAsync(dto.ArticleId);
            return Ok(new { success = true, isPraised = result.Data, praiseCount = count });
        }

        [HttpPost("collect")]
        public async Task<IActionResult> Collect([FromBody] ToggleStateDto dto)
        {
            if (dto == null || dto.ArticleId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid articleId." });
            }

            var uid = CurrentUserId();
            if (uid is null)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var had = await _praiseCollectRepository.HasUserCollectedAsync(uid.Value, dto.ArticleId);
            await _praiseCollectRepository.UpdateCollectStatusAsync(uid.Value, dto.ArticleId, !had);

            var count = await _praiseCollectRepository.CountCollectionsAsync(dto.ArticleId);
            return Ok(new { success = true, isCollected = !had, collectCount = count });
        }
    }
}
