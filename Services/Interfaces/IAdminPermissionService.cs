namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 管理員權限檢查服務介面
    /// </summary>
    public interface IAdminPermissionService
    {
        /// <summary>
        /// 檢查是否可以創建指定角色的用戶
        /// </summary>
        /// <param name="currentUserRole">當前用戶角色</param>
        /// <param name="targetRole">要創建的用戶角色</param>
        /// <returns>是否允許創建</returns>
        bool CanCreateUser(int currentUserRole, int targetRole);

        /// <summary>
        /// 檢查是否可以編輯指定用戶
        /// </summary>
        /// <param name="currentUserId">當前用戶ID</param>
        /// <param name="currentUserRole">當前用戶角色</param>
        /// <param name="targetUserId">目標用戶ID</param>
        /// <param name="targetUserRole">目標用戶角色</param>
        /// <param name="newRole">要更新的新角色（可選）</param>
        /// <returns>權限檢查結果</returns>
        Task<(bool CanEdit, string? ErrorMessage)> CanEditUserAsync(
            Guid currentUserId, int currentUserRole, 
            Guid targetUserId, int targetUserRole, 
            int? newRole = null);

        /// <summary>
        /// 檢查是否可以刪除指定用戶
        /// </summary>
        /// <param name="currentUserId">當前用戶ID</param>
        /// <param name="currentUserRole">當前用戶角色</param>
        /// <param name="targetUserId">目標用戶ID</param>
        /// <param name="targetUserRole">目標用戶角色</param>
        /// <returns>權限檢查結果</returns>
        Task<(bool CanDelete, string? ErrorMessage)> CanDeleteUserAsync(
            Guid currentUserId, int currentUserRole, 
            Guid targetUserId, int targetUserRole);

        /// <summary>
        /// 獲取當前用戶的權限信息
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>權限信息</returns>
        Task<AdminPermissionDto> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// 檢查超級管理員數量
        /// </summary>
        /// <returns>超級管理員數量</returns>
        Task<int> GetSuperAdminCountAsync();
    }

    /// <summary>
    /// 管理員權限資訊 DTO
    /// </summary>
    public class AdminPermissionDto
    {
        /// <summary>
        /// 用戶 ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用戶角色
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// 角色名稱
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// 可以創建的角色列表
        /// </summary>
        public int[] CanCreateRoles { get; set; } = Array.Empty<int>();

        /// <summary>
        /// 權限標記
        /// </summary>
        public AdminPermissionFlags Permissions { get; set; } = new();
    }

    /// <summary>
    /// 管理員權限標記
    /// </summary>
    public class AdminPermissionFlags
    {
        /// <summary>
        /// 是否可以創建管理員
        /// </summary>
        public bool CanCreateAdmin { get; set; }

        /// <summary>
        /// 是否可以創建超級管理員
        /// </summary>
        public bool CanCreateSuperAdmin { get; set; }

        /// <summary>
        /// 是否可以編輯管理員
        /// </summary>
        public bool CanEditAdmin { get; set; }

        /// <summary>
        /// 是否可以編輯超級管理員
        /// </summary>
        public bool CanEditSuperAdmin { get; set; }

        /// <summary>
        /// 是否可以刪除管理員
        /// </summary>
        public bool CanDeleteAdmin { get; set; }

        /// <summary>
        /// 是否可以刪除超級管理員
        /// </summary>
        public bool CanDeleteSuperAdmin { get; set; }

        /// <summary>
        /// 是否可以修改自己的角色
        /// </summary>
        public bool CanModifyOwnRole { get; set; }

        /// <summary>
        /// 是否可以刪除自己
        /// </summary>
        public bool CanDeleteSelf { get; set; }
    }
}