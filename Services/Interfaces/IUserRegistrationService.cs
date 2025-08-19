using Matrix.ViewModels;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 用戶註冊服務介面
    /// </summary>
    public interface IUserRegistrationService
    {
        /// <summary>
        /// 驗證註冊輸入資料
        /// </summary>
        /// <param name="model">註冊資料</param>
        /// <returns>驗證錯誤字典</returns>
        Dictionary<string, string[]> ValidateRegistrationInput(RegisterViewModel model);

        /// <summary>
        /// 註冊用戶
        /// </summary>
        /// <param name="model">註冊資料</param>
        /// <param name="role">用戶角色 (0=一般用戶, 1=管理員, 2=超級管理員)</param>
        /// <returns>用戶ID和錯誤列表</returns>
        Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(RegisterViewModel? model, int role = 0);

        /// <summary>
        /// 處理來自 UserService 的錯誤映射
        /// </summary>
        /// <param name="errors">UserService 回傳的錯誤列表</param>
        /// <returns>映射到前端欄位的錯誤字典</returns>
        Dictionary<string, string[]> MapServiceErrorsToFieldErrors(List<string> errors);
    }
}