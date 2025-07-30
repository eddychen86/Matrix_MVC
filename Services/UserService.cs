using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 使用者服務 - 重構版本使用 Repository Pattern
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOptions<UserValidationOptions> _validationOptions;

        public UserService(IUserRepository userRepository, IOptions<UserValidationOptions> validationOptions)
        {
            _userRepository = userRepository;
            _validationOptions = validationOptions;
        }

        /// <summary>
        /// 根據ID獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserWithPersonAsync(id);

            if (user?.Person == null) return null;

            return MapToUserDto(user);
        }

        /// <summary>
        /// 根據Email獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user?.Person == null) return null;

            return MapToUserDto(user);
        }

        /// <summary>
        /// 根據使用者名稱獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user?.Person == null) return null;

            return MapToUserDto(user);
        }

        /// <summary>
        /// 驗證使用者登入（支援 email 或用戶名）
        /// </summary>
        public async Task<bool> ValidateUserAsync(string emailOrUsername, string password)
        {
            return await _userRepository.ValidateUserAsync(emailOrUsername, password);
        }

        /// <summary>
        /// 建立使用者
        /// </summary>
        public async Task<(Guid? UserId, List<string> Errors)> CreateUserAsync(CreateUserDto dto)
        {
            var errors = new List<string>();
            
            // 驗證用戶名
            var userNameValidation = ValidateUserName(dto.UserName);
            if (!userNameValidation.IsValid)
                errors.Add(userNameValidation.ErrorMessage);
            
            // 驗證密碼
            var passwordValidation = ValidatePassword(dto.Password);
            if (!passwordValidation.IsValid)
                errors.Add(passwordValidation.ErrorMessage);
            
            // 驗證Email
            var emailValidation = ValidateEmail(dto.Email);
            if (!emailValidation.IsValid)
                errors.Add(emailValidation.ErrorMessage);
            
            // 檢查是否已存在
            if (await _userRepository.EmailExistsAsync(dto.Email))
                errors.Add("Email已被使用");
            
            if (await _userRepository.UsernameExistsAsync(dto.UserName))
                errors.Add("用戶名已被使用");
            
            if (errors.Count > 0)
                return (null, errors);

            // 先創建 User
            var user = new User
            {
                Role = 0,
                UserName = dto.UserName,
                Email = dto.Email,
                Password = HashPassword(dto.Password),
                CreateTime = DateTime.Now,
                Status = 0
            };

            // 添加並保存 User
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // 創建對應的 Person (這裡需要 PersonRepository)
            // TODO: 實作 PersonRepository 後再處理
            
            return (user.UserId, new List<string>());
        }

        /// <summary>
        /// 檢查Email是否存在
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        /// <summary>
        /// 檢查使用者名稱是否存在
        /// </summary>
        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            return await _userRepository.UsernameExistsAsync(userName);
        }

        /// <summary>
        /// 更新使用者狀態
        /// </summary>
        public async Task<bool> UpdateUserStatusAsync(Guid id, int status)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            user.Status = status;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新狀態
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            return await UpdateUserStatusAsync(id, status);
        }

        #region 驗證方法

        /// <summary>
        /// 驗證用戶名
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateUserName(string userName)
        {
            var options = _validationOptions.Value.UserName;
            
            if (string.IsNullOrEmpty(userName))
                return (false, "用戶名不能為空");
            
            if (userName.Length < options.RequiredLength)
                return (false, $"用戶名長度不能少於 {options.RequiredLength} 字元");
            
            if (userName.Length > options.MaximumLength)
                return (false, $"用戶名長度不能超過 {options.MaximumLength} 字元");
            
            if (!userName.All(c => options.AllowedCharacters.Contains(c)))
                return (false, "用戶名只能包含字母、數字和底線");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            var options = _validationOptions.Value.Password;
            var specialCharPattern = @"[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]";
            
            if (string.IsNullOrEmpty(password))
            return (false, "密碼不能為空");
            
            if (password.Length < options.RequiredLength)
                return (false, $"密碼長度不能少於 {options.RequiredLength} 字元");
            
            if (password.Length > options.MaximumLength)
                return (false, $"密碼長度不能超過 {options.MaximumLength} 字元");
            
            if (options.RequireDigit && !password.Any(char.IsDigit))
                return (false, "密碼必須包含至少一個數字");
            
            if (options.RequireLowercase && !password.Any(char.IsLower))
                return (false, "密碼必須包含至少一個小寫字母");
            
            if (options.RequireUppercase && !password.Any(char.IsUpper))
                return (false, "密碼必須包含至少一個大寫字母");
            
            if (options.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
                return (false, "密碼必須包含至少一個特殊字元");

            if (options.RequireNonAlphanumeric && !Regex.IsMatch(password, specialCharPattern))
                return (false, "密碼必須包含至少一個特殊符號(!@#$%^&*等)");
            
            if (password.Distinct().Count() < options.RequiredUniqueChars)
            return (false, $"密碼必須包含至少 {options.RequiredUniqueChars} 個不同的字元");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// 驗證Email
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateEmail(string email)
        {
            var options = _validationOptions.Value.Email;
            
            if (string.IsNullOrEmpty(email))
                return (false, "Email不能為空");
            
            if (email.Length > options.MaximumLength)
                return (false, $"Email長度不能超過 {options.MaximumLength} 字元");
            
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
                return (false, "Email格式不正確");
            
            return (true, string.Empty);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 將 User 實體轉換為 UserDto
        /// </summary>
        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                UserName = user.UserName,
                Email = user.Email,
                CreateTime = user.CreateTime,
                Status = user.Status,
                Person = new PersonDto
                {
                    PersonId = user.Person!.PersonId,
                    UserId = user.Person.UserId,
                    DisplayName = user.Person.DisplayName,
                    Bio = user.Person.Bio,
                    AvatarPath = user.Person.AvatarPath,
                    BannerPath = user.Person.BannerPath,
                    ExternalUrl = user.Person.ExternalUrl,
                    IsPrivate = user.Person.IsPrivate,
                    WalletAddress = user.Person.WalletAddress,
                    ModifyTime = user.Person.ModifyTime
                }
            };
        }

        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(password + "salt")));
        }

        #endregion

        #region 暫時未實作的方法 - 需要額外的 Repository

        public Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            throw new NotImplementedException("需要 PersonRepository 來實作此功能");
        }

        public Task<bool> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException("此方法需要重構");
        }

        public Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            throw new NotImplementedException("此方法需要重構");
        }

        public Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 20, string? searchKeyword = null)
        {
            throw new NotImplementedException("需要實作分頁查詢方法");
        }

        public Task<(List<UserDto> Items, int TotalCount)> SearchAsync(string? keyword = null, int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException("需要實作搜尋方法");
        }

        #endregion
    }
}