using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 使用者服務介面
    /// 定義使用者相關的業務邏輯操作
    /// </summary>
    public interface IUserService : ISearchableService<UserDto>, IStatusManageable<Guid>
    {
        /// <summary>
        /// 根據 ID 獲取使用者資料
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
        Task<UserDto?> GetUserAsync(Guid id);

        /// <summary>
        /// 根據電子郵件獲取使用者資料
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
        Task<UserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// 建立新使用者
        /// </summary>
        /// <param name="dto">建立使用者資料傳輸物件</param>
        /// <returns>建立是否成功</returns>
        Task<bool> CreateUserAsync(CreateUserDto dto);

        /// <summary>
        /// 更新使用者資料
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="dto">更新使用者資料傳輸物件</param>
        /// <returns>更新是否成功</returns>
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto);

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>刪除是否成功</returns>
        Task<bool> DeleteUserAsync(Guid id);

        /// <summary>
        /// 分頁查詢使用者列表
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <param name="searchKeyword">搜尋關鍵字</param>
        /// <returns>使用者列表和總數量</returns>
        Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(
            int page = 1,
            int pageSize = 20,
            string? searchKeyword = null);

        /// <summary>
        /// 驗證使用者登入
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="password">密碼</param>
        /// <returns>驗證是否成功</returns>
        Task<bool> ValidateUserAsync(string email, string password);

        /// <summary>
        /// 重設使用者密碼
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>重設是否成功</returns>
        Task<bool> ResetPasswordAsync(string email, string newPassword);

        /// <summary>
        /// 檢查電子郵件是否已存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsEmailExistsAsync(string email);

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// </summary>
        /// <param name="userName">使用者名稱</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsUserNameExistsAsync(string userName);
    }
}