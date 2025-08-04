using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 統一的授權檢查服務介面
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// 檢查用戶是否通過完整的授權驗證
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="minimumRole">最低權限等級（可選）</param>
        /// <returns>授權結果</returns>
        Task<AuthorizationResult> CheckUserAuthorizationAsync(Guid userId, int? minimumRole = null);

        /// <summary>
        /// 檢查用戶帳號狀態是否正常
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <returns>是否為正常狀態</returns>
        Task<bool> IsUserActiveAsync(Guid userId);

        /// <summary>
        /// 檢查用戶是否具有指定權限等級
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="requiredRole">需要的權限等級</param>
        /// <returns>是否具有權限</returns>
        Task<bool> HasPermissionAsync(Guid userId, int requiredRole);

        /// <summary>
        /// 獲取用戶的授權信息
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <returns>用戶授權信息</returns>
        Task<UserAuthInfo?> GetUserAuthInfoAsync(Guid userId);

        /// <summary>
        /// 檢查用戶狀態並返回適當的錯誤訊息
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <returns>狀態檢查結果</returns>
        Task<UserStatusResult> CheckUserStatusAsync(Guid userId);
    }

    /// <summary>
    /// 授權檢查結果
    /// </summary>
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string? ErrorMessage { get; set; }
        public UserAuthInfo? UserInfo { get; set; }
        public UserStatusCode StatusCode { get; set; }

        public static AuthorizationResult Success(UserAuthInfo userInfo)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                UserInfo = userInfo,
                StatusCode = UserStatusCode.Active
            };
        }

        public static AuthorizationResult Fail(string errorMessage, UserStatusCode statusCode)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }
    }

    /// <summary>
    /// 用戶授權信息
    /// </summary>
    public class UserAuthInfo
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public int Status { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsAdmin => Role >= 1;
        public bool IsMember => Role >= 0;
    }

    /// <summary>
    /// 用戶狀態檢查結果
    /// </summary>
    public class UserStatusResult
    {
        public bool IsValid { get; set; }
        public UserStatusCode StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public UserAuthInfo? UserInfo { get; set; }

        public static UserStatusResult Success(UserAuthInfo userInfo)
        {
            return new UserStatusResult
            {
                IsValid = true,
                StatusCode = UserStatusCode.Active,
                UserInfo = userInfo
            };
        }

        public static UserStatusResult Fail(UserStatusCode statusCode, string errorMessage)
        {
            return new UserStatusResult
            {
                IsValid = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// 用戶狀態代碼
    /// </summary>
    public enum UserStatusCode
    {
        /// <summary>用戶不存在</summary>
        NotFound = 0,
        /// <summary>帳號未確認</summary>
        Unconfirmed = 1,
        /// <summary>帳號正常</summary>
        Active = 2,
        /// <summary>帳號被封禁</summary>
        Banned = 3,
        /// <summary>權限不足</summary>
        InsufficientPermission = 4
    }
}