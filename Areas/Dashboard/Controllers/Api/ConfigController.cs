using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matrix.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Matrix.Services.Interfaces;
using Matrix.DTOs;
using Matrix.ViewModels;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorization]
    public class ConfigController(
        IUserService userService,
        IUserRegistrationService registrationService,
        IAdminPermissionService permissionService,
        ILogger<ConfigController> logger,
        ICustomLocalizer localizer
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IUserRegistrationService _registrationService = registrationService;
        private readonly IAdminPermissionService _permissionService = permissionService;
        private readonly ILogger<ConfigController> _logger = logger;
        private readonly ICustomLocalizer _localizer = localizer;

        /// <summary>
        /// 獲取當前登入用戶資訊
        /// </summary>
        private async Task<(Guid UserId, int Role)?> GetCurrentUserInfoAsync()
        {
            try
            {
                // 從 Claims 中獲取用戶 ID
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return null;
                }

                // 獲取用戶詳細資訊
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return null;
                }

                return (userId, user.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取當前用戶資訊失敗");
                return null;
            }
        }

        // GET: /api/Config/Permissions
        [HttpGet("Permissions")]
        public async Task<IActionResult> GetCurrentUserPermissionsAsync()
        {
            var currentUser = await GetCurrentUserInfoAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "無法獲取用戶資訊" });
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(currentUser.Value.UserId);
            
            return Ok(new
            {
                success = true,
                data = permissions
            });
        }

        // POST: /api/Config
        [HttpPost("")]
        public async Task<IActionResult> GetAllAdminAsync([FromBody] UserParamDto dto)
        {
            return Ok(new
            {
                success = true,
                data = await _userService.GetAdminAsync(dto.pages, dto.pageSize),
            });
        }

        // POST: /api/Config/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateAdminAsync([FromBody] CreateAdminDto model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "無效的請求資料" });
            }

            _logger.LogInformation("管理員建立嘗試: {UserName}, 目標角色: {Role}", model.UserName, model.Role);

            // 獲取當前用戶資訊
            var currentUser = await GetCurrentUserInfoAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "無法獲取用戶資訊" });
            }

            // 檢查創建權限
            if (!_permissionService.CanCreateUser(currentUser.Value.Role, model.Role))
            {
                var errorMessage = currentUser.Value.Role == 1 
                    ? "管理員只能創建管理員帳號，不能創建超級管理員"
                    : "您沒有創建該角色用戶的權限";
                
                return StatusCode(403, new { success = false, message = errorMessage });
            }

            // 創建 RegisterViewModel 來使用共用的註冊服務
            var registerModel = new RegisterViewModel
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm,
                DisplayName = model.DisplayName
            };

            // 使用註冊服務建立管理員（角色 = 1 或 2）
            var (userId, errors) = await _registrationService.RegisterUserAsync(registerModel, role: model.Role);

            if (userId == null)
            {
                _logger.LogWarning("管理員建立失敗: {Errors}", string.Join(", ", errors));
                var fieldErrors = _registrationService.MapServiceErrorsToFieldErrors(errors);
                
                return BadRequest(new 
                { 
                    success = false, 
                    message = "建立管理員失敗", 
                    errors = fieldErrors 
                });
            }

            _logger.LogInformation("管理員建立成功: {UserName}, UserId: {UserId}, 角色: {Role}", 
                model.UserName, userId, model.Role);

            // 管理員帳號不需要發送驗證信，直接建立完成
            return Ok(new
            {
                success = true,
                message = model.Role == 2 ? "超級管理員帳號建立成功" : "管理員帳號建立成功",
                data = new { 
                    userId = userId, 
                    role = model.Role,
                    roleText = model.Role == 2 ? "超級管理員" : "管理員",
                    status = "已啟用" // 管理員帳號預設啟用
                }
            });
        }

        // PUT: /api/Config/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateAdminAsync(Guid id, [FromBody] UpdateAdminDto model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "無效的請求資料" });
            }

            _logger.LogInformation("管理員更新嘗試: {TargetUserId}, 新角色: {NewRole}", id, model.Role);

            // 獲取當前用戶資訊
            var currentUser = await GetCurrentUserInfoAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "無法獲取用戶資訊" });
            }

            // 獲取目標用戶資訊
            var targetUser = await _userService.GetUserAsync(id);
            if (targetUser == null)
            {
                return NotFound(new { success = false, message = "找不到指定的用戶" });
            }

            // 檢查編輯權限
            var (canEdit, errorMessage) = await _permissionService.CanEditUserAsync(
                currentUser.Value.UserId, currentUser.Value.Role,
                id, targetUser.Role, model.Role);

            if (!canEdit)
            {
                return StatusCode(403, new { success = false, message = errorMessage ?? "您沒有編輯該用戶的權限" });
            }

            // TODO: 實作更新邏輯
            // 這裡需要實作實際的用戶更新邏輯

            return Ok(new
            {
                success = true,
                message = "管理員資料更新成功"
            });
        }

        // DELETE: /api/Config/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAdminAsync(Guid id)
        {
            _logger.LogInformation("管理員刪除嘗試: {TargetUserId}", id);

            // 獲取當前用戶資訊
            var currentUser = await GetCurrentUserInfoAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "無法獲取用戶資訊" });
            }

            // 獲取目標用戶資訊
            var targetUser = await _userService.GetUserAsync(id);
            if (targetUser == null)
            {
                return NotFound(new { success = false, message = "找不到指定的用戶" });
            }

            // 檢查刪除權限
            var (canDelete, errorMessage) = await _permissionService.CanDeleteUserAsync(
                currentUser.Value.UserId, currentUser.Value.Role,
                id, targetUser.Role);

            if (!canDelete)
            {
                return StatusCode(403, new { success = false, message = errorMessage ?? "您沒有刪除該用戶的權限" });
            }

            // TODO: 實作刪除邏輯
            // 這裡需要實作實際的用戶刪除邏輯

            return Ok(new
            {
                success = true,
                message = "管理員刪除成功"
            });
        }
    }
}