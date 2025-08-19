using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 管理員權限檢查服務實作
    /// </summary>
    public class AdminPermissionService : IAdminPermissionService
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminPermissionService> _logger;

        public AdminPermissionService(
            IUserService userService,
            ILogger<AdminPermissionService> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 檢查是否可以創建指定角色的用戶
        /// </summary>
        public bool CanCreateUser(int currentUserRole, int targetRole)
        {
            // 超級管理員可以創建任何角色
            if (currentUserRole == 2)
            {
                return targetRole == 1 || targetRole == 2;
            }

            // 管理員只能創建管理員
            if (currentUserRole == 1)
            {
                return targetRole == 1;
            }

            // 一般用戶無權限
            return false;
        }

        /// <summary>
        /// 檢查是否可以編輯指定用戶
        /// </summary>
        public async Task<(bool CanEdit, string? ErrorMessage)> CanEditUserAsync(
            Guid currentUserId, int currentUserRole, 
            Guid targetUserId, int targetUserRole, 
            int? newRole = null)
        {
            // 超級管理員可以編輯所有用戶
            if (currentUserRole == 2)
            {
                // 如果是要修改自己的角色，且只有一個超級管理員，則不允許
                if (currentUserId == targetUserId && newRole.HasValue && newRole.Value != 2)
                {
                    var superAdminCount = await GetSuperAdminCountAsync();
                    if (superAdminCount <= 1)
                    {
                        return (false, "您是唯一的超級管理員，不能降低自己的權限");
                    }
                }
                return (true, null);
            }

            // 管理員只能編輯同為管理員的用戶，且不能修改角色到超級管理員
            if (currentUserRole == 1)
            {
                if (targetUserRole != 1)
                {
                    return (false, "您沒有權限編輯超級管理員");
                }

                if (newRole.HasValue && newRole.Value == 2)
                {
                    return (false, "您沒有權限將用戶提升為超級管理員");
                }

                return (true, null);
            }

            return (false, "您沒有管理員權限");
        }

        /// <summary>
        /// 檢查是否可以刪除指定用戶
        /// </summary>
        public async Task<(bool CanDelete, string? ErrorMessage)> CanDeleteUserAsync(
            Guid currentUserId, int currentUserRole, 
            Guid targetUserId, int targetUserRole)
        {
            // 管理員無權刪除任何用戶
            if (currentUserRole == 1)
            {
                return (false, "管理員沒有刪除其他管理員的權限");
            }

            // 超級管理員可以刪除其他用戶
            if (currentUserRole == 2)
            {
                // 如果要刪除自己，且只有一個超級管理員，則不允許
                if (currentUserId == targetUserId)
                {
                    var superAdminCount = await GetSuperAdminCountAsync();
                    if (superAdminCount <= 1)
                    {
                        return (false, "您是唯一的超級管理員，不能刪除自己");
                    }
                }
                return (true, null);
            }

            return (false, "您沒有刪除權限");
        }

        /// <summary>
        /// 獲取當前用戶的權限信息
        /// </summary>
        public async Task<AdminPermissionDto> GetUserPermissionsAsync(Guid userId)
        {
            var user = await _userService.GetUserAsync(userId);
            if (user == null)
            {
                return new AdminPermissionDto
                {
                    UserId = userId,
                    Role = 0,
                    RoleName = "未知",
                    CanCreateRoles = Array.Empty<int>(),
                    Permissions = new AdminPermissionFlags()
                };
            }

            var superAdminCount = await GetSuperAdminCountAsync();
            var isLastSuperAdmin = user.Role == 2 && superAdminCount <= 1;

            var permissions = new AdminPermissionFlags();

            // 根據角色設定權限
            if (user.Role == 2) // 超級管理員
            {
                permissions.CanCreateAdmin = true;
                permissions.CanCreateSuperAdmin = true;
                permissions.CanEditAdmin = true;
                permissions.CanEditSuperAdmin = true;
                permissions.CanDeleteAdmin = true;
                permissions.CanDeleteSuperAdmin = true;
                permissions.CanModifyOwnRole = !isLastSuperAdmin; // 最後一個超級管理員不能修改自己角色
                permissions.CanDeleteSelf = !isLastSuperAdmin; // 最後一個超級管理員不能刪除自己
            }
            else if (user.Role == 1) // 管理員
            {
                permissions.CanCreateAdmin = true;
                permissions.CanCreateSuperAdmin = false;
                permissions.CanEditAdmin = true;
                permissions.CanEditSuperAdmin = false;
                permissions.CanDeleteAdmin = false;
                permissions.CanDeleteSuperAdmin = false;
                permissions.CanModifyOwnRole = false; // 管理員不能修改自己角色
                permissions.CanDeleteSelf = false; // 管理員不能刪除自己
            }

            return new AdminPermissionDto
            {
                UserId = userId,
                Role = user.Role,
                RoleName = GetRoleName(user.Role),
                CanCreateRoles = GetAvailableCreateRoles(user.Role),
                Permissions = permissions
            };
        }

        /// <summary>
        /// 檢查超級管理員數量
        /// </summary>
        public async Task<int> GetSuperAdminCountAsync()
        {
            try
            {
                // 這裡需要從 UserService 或 Repository 獲取超級管理員數量
                // 暫時先返回假設值，實際實作需要查詢資料庫
                var admins = await _userService.GetAdminAsync(1, 100); // 獲取足夠多的管理員
                return admins.Count(a => a.Role == 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取超級管理員數量時發生錯誤");
                return 1; // 預設值，保護機制
            }
        }

        /// <summary>
        /// 獲取角色名稱
        /// </summary>
        private static string GetRoleName(int role)
        {
            return role switch
            {
                0 => "一般用戶",
                1 => "管理員",
                2 => "超級管理員",
                _ => "未知角色"
            };
        }

        /// <summary>
        /// 獲取可以創建的角色列表
        /// </summary>
        private static int[] GetAvailableCreateRoles(int currentUserRole)
        {
            return currentUserRole switch
            {
                2 => new[] { 1, 2 }, // 超級管理員可以創建管理員和超級管理員
                1 => new[] { 1 },    // 管理員只能創建管理員
                _ => Array.Empty<int>() // 一般用戶無權限
            };
        }
    }
}