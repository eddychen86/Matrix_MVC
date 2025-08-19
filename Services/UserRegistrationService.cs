using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Matrix.DTOs;

namespace Matrix.Services
{
    /// <summary>
    /// 用戶註冊服務實作
    /// </summary>
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUserService _userService;
        private readonly ICustomLocalizer _localizer;
        private readonly ILogger<UserRegistrationService> _logger;

        public UserRegistrationService(
            IUserService userService,
            ICustomLocalizer localizer,
            ILogger<UserRegistrationService> logger)
        {
            _userService = userService;
            _localizer = localizer;
            _logger = logger;
        }

        /// <summary>
        /// 驗證註冊輸入資料
        /// </summary>
        public Dictionary<string, string[]> ValidateRegistrationInput(RegisterViewModel model)
        {
            var validationErrors = new Dictionary<string, string[]>();

            if (model == null)
            {
                validationErrors[""] = [_localizer["Error"]];
                return validationErrors;
            }

            // 用戶名驗證
            if (string.IsNullOrWhiteSpace(model.UserName))
                validationErrors["UserName"] = [_localizer["UserNameInvalid"]];
            else if (model.UserName.Length < 3 || model.UserName.Length > 20)
                validationErrors["UserName"] = [_localizer["UserNameFormatError"]];

            // 郵件驗證
            if (string.IsNullOrWhiteSpace(model.Email))
                validationErrors["Email"] = [_localizer["EmailRequired"]];
            else if (!model.Email.Contains('@') || !model.Email.Contains('.'))
                validationErrors["Email"] = [_localizer["EmailInvalid"]];
            else if (model.Email.Length > 30)
                validationErrors["Email"] = [_localizer["EmailFormatError"]];

            // 密碼驗證
            if (string.IsNullOrWhiteSpace(model.Password))
                validationErrors["Password"] = [_localizer["PasswordInvalid"]];
            else if (model.Password.Length < 8 || model.Password.Length > 20)
                validationErrors["Password"] = [_localizer["PasswordFormatError"]];

            // 確認密碼驗證
            if (string.IsNullOrWhiteSpace(model.PasswordConfirm))
                validationErrors["PasswordConfirm"] = [_localizer["PasswordConfirmRequired"]];
            else if (model.Password != model.PasswordConfirm)
                validationErrors["PasswordConfirm"] = [_localizer["PasswordCompareError"]];

            return validationErrors;
        }

        /// <summary>
        /// 註冊用戶
        /// </summary>
        public async Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(RegisterViewModel? model, int role = 0)
        {
            _logger.LogInformation("用戶註冊嘗試: {UserName}, 角色: {Role}", model?.UserName, role);

            // 檢查 model 是否為 null
            if (model == null)
            {
                return (null, new List<string> { _localizer["Error"] });
            }

            // 前端驗證
            var validationErrors = ValidateRegistrationInput(model);
            if (validationErrors.Count > 0)
            {
                var errorMessages = validationErrors.SelectMany(kv => kv.Value).ToList();
                return (null, errorMessages);
            }

            // 建立 DTO
            var createUserDto = new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email ?? string.Empty,
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm,
                Role = role  // 設定角色
            };

            // 呼叫 UserService 建立用戶
            var (userId, errors) = await _userService.CreateUserAsync(createUserDto);

            if (userId != null)
            {
                _logger.LogInformation("用戶註冊成功: {UserName}, UserId: {UserId}, 角色: {Role}", 
                    model.UserName, userId, role);
            }
            else
            {
                _logger.LogWarning("用戶註冊失敗: {UserName}, 錯誤: {Errors}", 
                    model.UserName, string.Join(", ", errors));
            }

            return (userId, errors);
        }

        /// <summary>
        /// 處理來自 UserService 的錯誤映射
        /// </summary>
        public Dictionary<string, string[]> MapServiceErrorsToFieldErrors(List<string> errors)
        {
            var fieldErrors = new Dictionary<string, string[]>();

            foreach (var error in errors)
            {
                // 根據錯誤內容映射到正確的欄位，使用多語系訊息
                if (error.Contains("用戶名") || error.Contains("UserName") || error.Contains("username"))
                {
                    if (error.Contains("長度") || error.Contains("字"))
                        fieldErrors["UserName"] = [_localizer["UserNameFormatError"]];
                    else if (error.Contains("已被使用") || error.Contains("exists"))
                        fieldErrors["UserName"] = [_localizer["UsernameExists"]];
                    else
                        fieldErrors["UserName"] = [_localizer["UserNameInvalid"]];
                }
                else if (error.Contains("郵件") || error.Contains("Email") || error.Contains("email"))
                {
                    if (error.Contains("格式") || error.Contains("invalid"))
                        fieldErrors["Email"] = [_localizer["EmailInvalid"]];
                    else if (error.Contains("已被使用") || error.Contains("exists"))
                        fieldErrors["Email"] = [_localizer["EmailExists"]];
                    else if (error.Contains("必填") || error.Contains("required"))
                        fieldErrors["Email"] = [_localizer["EmailRequired"]];
                    else
                        fieldErrors["Email"] = [_localizer["EmailFormatError"]];
                }
                else if (error.Contains("密碼") || error.Contains("Password") || error.Contains("password"))
                {
                    if (error.Contains("確認") || error.Contains("confirm") || error.Contains("不相符") || error.Contains("match"))
                        fieldErrors["PasswordConfirm"] = [_localizer["PasswordCompareError"]];
                    else if (error.Contains("格式") || error.Contains("大寫") || error.Contains("小寫") || error.Contains("數字") || error.Contains("特殊"))
                        fieldErrors["Password"] = [_localizer["PasswordFormatError"]];
                    else
                        fieldErrors["Password"] = [_localizer["PasswordInvalid"]];
                }
                else
                {
                    // 一般錯誤
                    fieldErrors[""] = [error];
                }
            }

            return fieldErrors;
        }
    }
}