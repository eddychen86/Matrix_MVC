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
        /// 根據使用者名稱獲取使用者資料
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
        Task<UserDto?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 根據使用者名稱或電子郵件獲取使用者資料
        /// </summary>
        /// <param name="identifier">使用者名稱或電子郵件</param>
        /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
        Task<UserDto?> GetUserByIdentifierAsync(string identifier);

        /// <summary>
        /// 建立新使用者
        /// </summary>
        /// <param name="dto">建立使用者資料傳輸物件</param>
        /// <returns>建立結果：使用者ID（成功時）和錯誤列表</returns>
        Task<(Guid? UserId, List<string> Errors)> CreateUserAsync(CreateUserDto dto);

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

        /// <summary>
        /// 直接更新用戶實體
        /// </summary>
        /// <param name="user">用戶實體</param>
        /// <returns>更新是否成功</returns>
        Task<bool> UpdateUserEntityAsync(User user);

        /// <summary>
        /// 根據 ID 獲取用戶實體
        /// </summary>
        /// <param name="id">用戶 ID</param>
        /// <returns>用戶實體，如果不存在則返回 null</returns>
        Task<User?> GetUserEntityAsync(Guid id);

        /// <summary>
        /// 更新使用者的個人資料，包含處理頭像和橫幅的檔案上傳
        /// </summary>
        Task<PersonDto?> UpdateProfileAsync(Guid userId, ApiUpdateProfileDto dto);

        /// <summary>
        /// 根據使用者 ID 獲取個人資料和相關文章
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>個人資料 DTO，包含相關文章</returns>
        Task<PersonDto?> GetProfileByIdAsync(Guid userId);

        /// <summary>
        /// 更新個人資料（包括密碼和網站連結）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">個人資料 DTO</param>
        /// <returns>更新結果</returns>
        Task<ReturnType<object>> UpdatePersonProfileAsync(Guid userId, PersonDto dto);

        /// <summary>
        /// 更新使用者狀態
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="status">新的狀態值</param>
        /// <returns>更新是否成功</returns>
        Task<bool> UpdateUserStatusAsync(Guid userId, int status);

        /// <summary>
        /// 更新個人資料圖片（頭像或橫幅）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="type">圖片類型：avatar 或 banner</param>
        /// <param name="filePath">檔案相對路徑</param>
        /// <returns>更新結果</returns>
        Task<ReturnType<object>> UpdateProfileImageAsync(Guid userId, string type, string filePath);

        /// <summary>
        /// 驗證密碼是否符合規則
        /// </summary>
        /// <param name="password">要驗證的密碼</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string ErrorMessage) ValidatePassword(string password);
    }
}