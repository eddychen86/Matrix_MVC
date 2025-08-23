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

        // 獲取當前登入用戶資訊
        private async Task<(Guid UserId, int Role)?> GetCurrentUserInfoAsync()
        {
            try
            { 
                // 嘗試從不同的 Claim 類型中獲取用戶 ID
                var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("UserId claim is missing or invalid: {UserIdClaim}", userIdClaim);
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

        // 獲取篩選後的管理員資料
        private async Task<(List<object> Admins, int TotalCount)> GetFilteredAdminsAsync(int page, int pageSize, AdminFilters? filters)
        {
            try
            {
                // 使用 GetUsersAsync 方法獲取所有用戶，然後在應用層篩選
                // 先獲取大量資料以便進行篩選，後續可以考慮在服務層實作更高效的方法
                var (allUsers, _) = await _userService.GetUsersAsync(page: 1, pageSize: 1000);

                // 篩選管理員（角色 >= 1）並排除軟刪除的用戶
                var adminUsers = allUsers.Where(u => u.Role >= 1 && u.IsDelete == 0).ToList();

                // 應用篩選條件
                if (filters != null)
                {
                    // 關鍵字搜尋（使用者名稱或信箱）
                    if (!string.IsNullOrWhiteSpace(filters.Keyword))
                    {
                        var keyword = filters.Keyword.Trim().ToLower();
                        adminUsers = adminUsers.Where(u => 
                            u.UserName.ToLower().Contains(keyword) || 
                            u.Email.ToLower().Contains(keyword) ||
                            (u.Person?.DisplayName?.ToLower().Contains(keyword) ?? false)).ToList();
                    }

                    // 超級管理員篩選
                    if (filters.SuperAdmin.HasValue)
                    {
                        if (filters.SuperAdmin.Value)
                            adminUsers = adminUsers.Where(u => u.Role == 2).ToList(); // 僅超級管理員
                        else
                            adminUsers = adminUsers.Where(u => u.Role == 1).ToList(); // 僅一般管理員
                    }

                    // 狀態篩選
                    if (filters.Status.HasValue)
                    {
                        if (filters.Status.Value)
                            adminUsers = adminUsers.Where(u => u.Status == 1).ToList(); // 已啟用
                        else
                            adminUsers = adminUsers.Where(u => u.Status == 0).ToList(); // 未啟用
                    }
                }

                // 計算總數
                var totalCount = adminUsers.Count;

                // 手動分頁
                var pagedUsers = adminUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // 轉換為前端需要的格式
                var adminList = pagedUsers.Select(u => new
                {
                    userId = u.UserId,
                    userName = u.UserName,
                    displayName = u.Person?.DisplayName ?? u.UserName,
                    email = u.Email,
                    avatarPath = u.Person?.AvatarPath,
                    role = u.Role,
                    status = u.Status,
                    createTime = u.CreateTime,
                    lastLoginTime = u.LastLoginTime
                }).ToList<object>();

                return (adminList, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取管理員列表時發生錯誤");
                throw; // 重新拋出異常讓上層處理
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
            
            return Ok(new { success = true, data = permissions } );
        }

        // POST: /api/Config
        [HttpPost("")]
        public async Task<IActionResult> GetAllAdminAsync([FromBody] AdminFilterDto dto)
        {
            try
            {
                // 修正分頁參數名稱 (前端傳 page，後端期望 pages)
                var pageNumber = dto.Page > 0 ? dto.Page : 1;
                var pageSize = dto.PageSize > 0 ? dto.PageSize : 10;

                // 獲取篩選後的管理員資料
                var (admins, totalCount) = await GetFilteredAdminsAsync(pageNumber, pageSize, dto.Filters);

                // 計算分頁資訊
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    success = true,
                    data = admins,
                    totalPages,
                    totalCount,
                    currentPage = pageNumber,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取管理員列表失敗");
                return StatusCode(500, new { success = false, message = "獲取管理員列表失敗" } );
            }
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
                var errorMessage = currentUser.Value.Role == 1 ? "管理員只能創建管理員帳號，不能創建超級管理員" : "您沒有創建該角色用戶的權限";
                
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
                
                return BadRequest(new { success = false, message = "建立管理員失敗", errors = fieldErrors } );
            }

            _logger.LogInformation("管理員建立成功: {UserName}, UserId: {UserId}, 角色: {Role}", model.UserName, userId, model.Role);

            // 管理員帳號不需要發送驗證信，直接建立完成
            return Ok(new
            {
                success = true,
                message = model.Role == 2 ? "超級管理員帳號建立成功" : "管理員帳號建立成功",
                data = new { 
                    userId, 
                    role = model.Role,
                    roleText = model.Role == 2 ? "超級管理員" : "管理員",
                    status = 1 // 管理員帳號預設啟用
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

            try
            {
                // 獲取原始用戶實體以進行更新
                var userEntity = await _userService.GetUserEntityAsync(id);
                if (userEntity == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的用戶" });
                }

                // 更新用戶資料
                bool hasUpdates = false;

                if (model.Role.HasValue && model.Role.Value != userEntity.Role)
                {
                    userEntity.Role = model.Role.Value;
                    hasUpdates = true;
                    _logger.LogInformation("更新用戶角色: {UserId} 從 {OldRole} 到 {NewRole}", 
                        id, targetUser.Role, model.Role.Value);
                }

                if (model.Status.HasValue && model.Status.Value != userEntity.Status)
                {
                    userEntity.Status = model.Status.Value;
                    hasUpdates = true;
                    _logger.LogInformation("更新用戶狀態: {UserId} 從 {OldStatus} 到 {NewStatus}", 
                        id, targetUser.Status, model.Status.Value);
                }

                if (model.IsDelete.HasValue && model.IsDelete.Value != userEntity.IsDelete)
                {
                    userEntity.IsDelete = model.IsDelete.Value;
                    hasUpdates = true;
                    _logger.LogInformation("更新用戶刪除狀態: {UserId} 從 {OldIsDelete} 到 {NewIsDelete}", 
                        id, targetUser.IsDelete, model.IsDelete.Value);
                }

                if (!hasUpdates)
                {
                    return BadRequest(new { success = false, message = "沒有需要更新的資料" });
                }

                // 執行更新
                var updateResult = await _userService.UpdateUserEntityAsync(userEntity);
                if (!updateResult)
                {
                    _logger.LogError("更新用戶失敗: {UserId}", id);
                    return StatusCode(500, new { success = false, message = "更新管理員資料失敗" });
                }

                _logger.LogInformation("管理員資料更新成功: {UserId}", id);

                return Ok(new
                {
                    success = true,
                    message = "管理員資料更新成功",
                    data = new
                    {
                        userId = id,
                        role = userEntity.Role,
                        status = userEntity.Status,
                        isDelete = userEntity.IsDelete
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新管理員資料時發生錯誤: {UserId}", id);
                return StatusCode(500, new { success = false, message = "更新管理員資料失敗" });
            }
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

            try
            {
                // 檢查是否嘗試刪除自己
                if (currentUser.Value.UserId == id)
                {
                    return BadRequest(new { success = false, message = "不能刪除自己的帳號" });
                }

                // 使用軟刪除以保持資料完整性
                var deleteResult = await _userService.SoftDeleteUserAsync(id);
                if (!deleteResult)
                {
                    _logger.LogError("軟刪除管理員失敗: {UserId}", id);
                    return StatusCode(500, new { success = false, message = "刪除管理員失敗" });
                }

                _logger.LogInformation("管理員軟刪除成功: {UserId}, 操作者: {OperatorId}", 
                    id, currentUser.Value.UserId);

                return Ok(new
                {
                    success = true,
                    message = "管理員刪除成功",
                    data = new
                    {
                        deletedUserId = id,
                        operatorId = currentUser.Value.UserId,
                        deleteTime = DateTime.UtcNow,
                        deleteType = "SoftDelete"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除管理員時發生錯誤: {UserId}", id);
                return StatusCode(500, new { success = false, message = "刪除管理員失敗" });
            }
        }
    }
}