using Matrix.DTOs;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 統一的授權檢查服務實作
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly ICustomLocalizer _localizer;

        public AuthorizationService(
            IUserService userService,
            ILogger<AuthorizationService> logger,
            ICustomLocalizer localizer)
        {
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
        }

        /// <summary>
        /// 檢查用戶是否通過完整的授權驗證
        /// </summary>
        public async Task<AuthorizationResult> CheckUserAuthorizationAsync(Guid userId, int? minimumRole = null)
        {
            try
            {
                // 獲取用戶信息
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("用戶不存在: {UserId}", userId);
                    return AuthorizationResult.Fail(_localizer["UserNotFound"], UserStatusCode.NotFound);
                }

                // 檢查用戶狀態
                var statusResult = MapUserStatusToCode(user.Status);
                if (statusResult != UserStatusCode.Active)
                {
                    var errorMessage = GetStatusErrorMessage(statusResult);
                    _logger.LogWarning("用戶狀態異常: {UserId}, Status: {Status}", userId, user.Status);
                    return AuthorizationResult.Fail(errorMessage, statusResult);
                }

                // 檢查權限等級
                if (minimumRole.HasValue && user.Role < minimumRole.Value)
                {
                    _logger.LogWarning("用戶權限不足: {UserId}, Role: {Role}, Required: {RequiredRole}", 
                        userId, user.Role, minimumRole.Value);
                    return AuthorizationResult.Fail(_localizer["InsufficientPermission"], UserStatusCode.InsufficientPermission);
                }

                // 創建用戶授權信息
                var authInfo = new UserAuthInfo
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    LastLoginTime = user.LastLoginTime
                };

                return AuthorizationResult.Success(authInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶授權時發生錯誤: {UserId}", userId);
                return AuthorizationResult.Fail(_localizer["AuthorizationCheckError"], UserStatusCode.NotFound);
            }
        }

        /// <summary>
        /// 檢查用戶帳號狀態是否正常
        /// </summary>
        public async Task<bool> IsUserActiveAsync(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                return user?.Status == 1; // 1 = 正常狀態
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶狀態時發生錯誤: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 檢查用戶是否具有指定權限等級
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, int requiredRole)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                if (user == null || user.Status != 1)
                {
                    return false;
                }

                return user.Role >= requiredRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶權限時發生錯誤: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 獲取用戶的授權信息
        /// </summary>
        public async Task<UserAuthInfo?> GetUserAuthInfoAsync(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return null;
                }

                return new UserAuthInfo
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    LastLoginTime = user.LastLoginTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶授權信息時發生錯誤: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// 檢查用戶狀態並返回適當的錯誤訊息
        /// </summary>
        public async Task<UserStatusResult> CheckUserStatusAsync(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return UserStatusResult.Fail(UserStatusCode.NotFound, _localizer["UserNotFound"]);
                }

                var statusCode = MapUserStatusToCode(user.Status);
                if (statusCode != UserStatusCode.Active)
                {
                    var errorMessage = GetStatusErrorMessage(statusCode);
                    return UserStatusResult.Fail(statusCode, errorMessage);
                }

                // 創建用戶授權信息
                var authInfo = new UserAuthInfo
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    LastLoginTime = user.LastLoginTime
                };

                return UserStatusResult.Success(authInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶狀態時發生錯誤: {UserId}", userId);
                return UserStatusResult.Fail(UserStatusCode.NotFound, _localizer["StatusCheckError"]);
            }
        }

        /// <summary>
        /// 將數據庫中的狀態值映射到狀態代碼
        /// </summary>
        private static UserStatusCode MapUserStatusToCode(int status)
        {
            return status switch
            {
                0 => UserStatusCode.Unconfirmed,  // 未確認
                1 => UserStatusCode.Active,       // 正常
                2 => UserStatusCode.Banned,       // 被封禁
                _ => UserStatusCode.NotFound      // 未知狀態
            };
        }

        /// <summary>
        /// 根據狀態代碼獲取錯誤訊息
        /// </summary>
        private string GetStatusErrorMessage(UserStatusCode statusCode)
        {
            return statusCode switch
            {
                UserStatusCode.NotFound => _localizer["UserNotFound"],
                UserStatusCode.Unconfirmed => _localizer["AccountNotConfirmed"],
                UserStatusCode.Banned => _localizer["AccountBanned"],
                UserStatusCode.InsufficientPermission => _localizer["InsufficientPermission"],
                _ => _localizer["AccountLoginError"]
            };
        }
    }
}