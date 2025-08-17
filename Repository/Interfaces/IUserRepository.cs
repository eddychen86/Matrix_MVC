using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 用戶資料存取介面
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>取得所有用戶</summary>
        Task<List<UserBasicDto>> GetAllWithUserAsync();

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
    }
}