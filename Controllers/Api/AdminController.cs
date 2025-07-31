using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.Controllers.Api;

namespace Matrix.Controllers.Api
{
    /// <summary>管理功能 API - 僅開發環境使用</summary>
    [Route("api/admin")]
    public class AdminController(IUserService _userService) : ApiControllerBase
    {
        /// <summary>檢查並修正用戶狀態</summary>
        [HttpPost("fix-user-status/{userId}")]
        public async Task<IActionResult> FixUserStatus(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return ApiError("用戶不存在");
                }

                if (user.Status != 1)
                {
                    await _userService.UpdateUserStatusAsync(userId, 1);
                    return ApiSuccess($"已修正用戶 {user.UserName} 的狀態：{user.Status} → 1");
                }

                return ApiSuccess($"用戶 {user.UserName} 狀態已正常：{user.Status}");
            }
            catch (Exception ex)
            {
                return ApiError($"修正失敗：{ex.Message}");
            }
        }

        /// <summary>檢查用戶狀態</summary>
        [HttpGet("check-user-status/{userId}")]
        public async Task<IActionResult> CheckUserStatus(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return ApiError("用戶不存在");
                }

                return ApiSuccess(new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    email = user.Email,
                    status = user.Status,
                    role = user.Role,
                    isStatusValid = user.Status == 1
                });
            }
            catch (Exception ex)
            {
                return ApiError($"檢查失敗：{ex.Message}");
            }
        }
    }
}