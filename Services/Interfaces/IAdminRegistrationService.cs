using Matrix.ViewModels;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 管理員註冊服務介面
    /// </summary>
    public interface IAdminRegistrationService
    {
        /// <summary>
        /// 驗證管理員註冊輸入資料
        /// </summary>
        /// <param name="model">管理員註冊資料</param>
        /// <returns>驗證錯誤字典</returns>
        Dictionary<string, string[]> ValidateRegistrationInput(AdminRegisterViewModel model);

        /// <summary>
        /// 註冊管理員用戶
        /// </summary>
        /// <param name="model">管理員註冊資料</param>
        /// <param name="role">用戶角色 (1=管理員, 2=超級管理員)</param>
        /// <returns>用戶ID和錯誤列表</returns>
        /// <remarks>
        /// 管理員註冊特殊規則:
        /// - Status 預設為 1 (已驗證，不需要點擊驗證信)
        /// - DisplayName 自動設為 UserName
        /// - Role 必須為 1 (管理員) 或 2 (超級管理員)
        /// </remarks>
        Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(AdminRegisterViewModel? model, int role = 1);

        /// <summary>
        /// 處理來自 UserService 的錯誤映射
        /// </summary>
        /// <param name="errors">UserService 回傳的錯誤列表</param>
        /// <returns>映射到前端欄位的錯誤字典</returns>
        Dictionary<string, string[]> MapServiceErrorsToFieldErrors(List<string> errors);
    }
}