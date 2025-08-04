using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.DTOs;

namespace Matrix.Controllers.Api
{
    /// <summary>
    /// 個人資料相關的 API Controller
    /// 提供個人資料的查詢和更新功能
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(
        IUserService userService,
        ILogger<ProfileController> logger
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<ProfileController> _logger = logger;

        /// <summary>
        /// 更新個人資料
        /// 更新使用者的個人資料，包括顯示名稱、自我介紹、網站連結和密碼
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="dto">個人資料 DTO</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}")]
        public async Task<ReturnType<object>> Update(Guid id, [FromBody] PersonDto dto)
        {
            try
            {
                _logger.LogInformation("開始更新個人資料，使用者 ID: {UserId}", id);
                
                var result = await _userService.UpdatePersonProfileAsync(id, dto);
                
                if (result.Success)
                {
                    _logger.LogInformation("個人資料更新成功，使用者 ID: {UserId}", id);
                }
                else
                {
                    _logger.LogWarning("個人資料更新失敗，使用者 ID: {UserId}，錯誤訊息: {Message}", id, result.Message);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新個人資料時發生例外，使用者 ID: {UserId}", id);
                return new ReturnType<object> 
                { 
                    Success = false, 
                    Message = "更新過程中發生錯誤，請稍後再試" 
                };
            }
        }

        /// <summary>
        /// 根據使用者 ID 獲取個人資料和相關文章
        /// 返回包含個人資料、使用者資訊和相關文章的完整資料
        /// </summary>
        /// <param name="model">包含使用者 ID 的請求模型</param>
        /// <returns>個人資料 DTO</returns>
        [HttpPost("get")]
        public async Task<ActionResult<PersonDto>> GetProfileById([FromBody] ProfileGetUserDto model) 
        {
            try
            {
                _logger.LogInformation("開始查詢個人資料，使用者 ID: {UserId}", model.id);
                
                var personDto = await _userService.GetProfileByIdAsync(model.id);
                
                if (personDto == null)
                {
                    _logger.LogWarning("找不到個人資料，使用者 ID: {UserId}", model.id);
                    return NotFound("找不到指定的個人資料");
                }
                
                _logger.LogInformation("成功查詢個人資料，使用者 ID: {UserId}", model.id);
                return Ok(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢個人資料時發生例外，使用者 ID: {UserId}", model.id);
                return StatusCode(500, "伺服器內部錯誤，請稍後再試");
            }
        }
    }
}