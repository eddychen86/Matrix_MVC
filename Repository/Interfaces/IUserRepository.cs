using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 用戶資料存取介面
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>取得所有用戶</summary>
        Task<(int totalUsers, int totalTodayLogin)> GetAllWithUserAsync();

        /// <summary>取得系統管理員</summary>
        Task<List<AdminDto>> GetAllWithAdminAsync(int pages, int pageSize);

        /// <summary>根據用戶名取得用戶</summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>根據電子郵件取得用戶</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>取得用戶及其個人資料</summary>
        Task<User?> GetUserWithPersonAsync(Guid userId);

        /// <summary>驗證用戶帳號密碼</summary>
        Task<bool> ValidateUserAsync(string username, string password);

        /// <summary>檢查用戶名是否存在</summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>檢查電子郵件是否存在</summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>根據識別符取得用戶（用戶名或電子郵件）</summary>
        Task<User?> GetByIdentifierAsync(string identifier);

        // === 軟刪除相關方法 ===
        
        /// <summary>軟刪除用戶（設定 IsDelete = 1）</summary>
        Task<bool> SoftDeleteAsync(Guid userId);

        /// <summary>恢復已刪除的用戶（設定 IsDelete = 0）</summary>
        Task<bool> RestoreAsync(Guid userId);

        /// <summary>取得所有未刪除的用戶</summary>
        Task<List<User>> GetActiveUsersAsync();

        /// <summary>取得所有已刪除的用戶</summary>
        Task<List<User>> GetDeletedUsersAsync();

        /// <summary>取得未刪除的管理員列表（分頁）</summary>
        Task<List<AdminDto>> GetActiveAdminsAsync(int pages, int pageSize);

        /// <summary>取得已刪除的管理員列表（分頁）</summary>
        Task<List<AdminDto>> GetDeletedAdminsAsync(int pages, int pageSize);

        /// <summary>檢查用戶是否已被軟刪除</summary>
        Task<bool> IsUserDeletedAsync(Guid userId);

        /// <summary>根據用戶名取得未刪除的用戶</summary>
        Task<User?> GetActiveByUsernameAsync(string username);

        /// <summary>根據電子郵件取得未刪除的用戶</summary>
        Task<User?> GetActiveByEmailAsync(string email);

        /// <summary>檢查用戶名是否在未刪除用戶中存在</summary>
        Task<bool> ActiveUsernameExistsAsync(string username);

        /// <summary>檢查電子郵件是否在未刪除用戶中存在</summary>
        Task<bool> ActiveEmailExistsAsync(string email);

        /// <summary>清除特定用戶的忘記密碼 Token</summary>
        Task<bool> ClearForgotPasswordTokenAsync(Guid userId);
    }
}