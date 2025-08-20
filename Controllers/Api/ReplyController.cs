using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Matrix.DTOs;
using Matrix.Services.Interfaces;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/reply")]
    public class ReplyController : ControllerBase
    {
        private readonly IReplyService _replyService;

        public ReplyController(IReplyService replyService)
        {
            _replyService = replyService;
        }

        private Guid? GetUserId()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var id) ? id : null;
        }

        [HttpGet("{articleId:guid}")]
        public async Task<IActionResult> GetReplies([FromRoute] Guid articleId)
        {
            var result = await _replyService.GetRepliesByArticleIdAsync(articleId);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data });
        }

        [Authorize]
        [HttpPost("{articleId}/reply")]
        public async Task<IActionResult> CreateReply(Guid articleId, [FromBody] CreateReplyDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var result = await _replyService.CreateReplyAsync(articleId, userId.Value, dto.Content);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}