using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Services
{
    /// <summary>
    /// 使用者服務
    /// </summary>
        public class UserService(ApplicationDbContext _context, IOptions<UserValidationOptions> _validationOptions) : IUserService
        {
            /// <summary>
            /// 根據ID獲取使用者
            /// </summary>
            public async Task<UserDto?> GetUserAsync(Guid id)
            {
                var user = await _context.Users
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.UserId == id);
    
                if (user?.Person == null) return null;
    
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
                        PersonId = user.Person.PersonId,
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
    
            /// <summary>
            /// 根據Email獲取使用者
            /// </summary>
            public async Task<UserDto?> GetUserByEmailAsync(string email)
            {
                var user = await _context.Users
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.Email == email);
    
                if (user?.Person == null) return null;
    
                return await GetUserAsync(user.UserId);
            }

            /// <summary>
            /// 根據使用者名稱獲取使用者
            /// </summary>
            public async Task<UserDto?> GetUserByUsernameAsync(string username)
            {
                var user = await _context.Users
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.UserName == username);

                if (user?.Person == null) return null;

                return await GetUserAsync(user.UserId);
            }
    
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
                if (await IsEmailExistsAsync(dto.Email))
                    errors.Add("Email已被使用");
                
                if (await IsUserNameExistsAsync(dto.UserName))
                    errors.Add("用戶名已被使用");
                
                if (errors.Count > 0)
                    return (null, errors);
    
                // 先創建 User（不包含 Person）
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
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 自動創建對應的 Person
                var person = new Person
                {
                    UserId = user.UserId, // 設定外鍵
                    DisplayName = dto.UserName,
                    ModifyTime = DateTime.Now
                };

                // 添加並保存 Person
                _context.Persons.Add(person);
                await _context.SaveChangesAsync();

                return (user.UserId, new List<string>());
            }

        /// <summary>
        /// 更新使用者
        /// </summary>
        public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user?.Person == null) return false;

            // 更新 User 相關欄位
            if (!string.IsNullOrEmpty(dto.UserName))
            {
                user.UserName = dto.UserName;
                // 如果沒有特別指定 DisplayName，就同步更新
                if (string.IsNullOrEmpty(dto.DisplayName))
                    user.Person.DisplayName = dto.UserName;
            }

            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Country))
                user.Country = dto.Country;

            if (dto.Gender.HasValue)
                user.Gender = dto.Gender;

            // 更新 Person 相關欄位
            if (!string.IsNullOrEmpty(dto.DisplayName))
                user.Person.DisplayName = dto.DisplayName;

            if (!string.IsNullOrEmpty(dto.Bio))
                user.Person.Bio = dto.Bio;

            if (!string.IsNullOrEmpty(dto.AvatarPath))
                user.Person.AvatarPath = dto.AvatarPath;

            if (!string.IsNullOrEmpty(dto.BannerPath))
                user.Person.BannerPath = dto.BannerPath;

            if (!string.IsNullOrEmpty(dto.ExternalUrl))
                user.Person.ExternalUrl = dto.ExternalUrl;

            if (dto.IsPrivate.HasValue)
                user.Person.IsPrivate = dto.IsPrivate.Value;

            if (!string.IsNullOrEmpty(dto.WalletAddress))
                user.Person.WalletAddress = dto.WalletAddress;

            // 更新修改時間
            user.Person.ModifyTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return false;

            user.Status = 2; // 已刪除
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 驗證使用者登入
        /// </summary>
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null && VerifyPassword(password, user.Password);
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新使用者狀態
        /// </summary>
        public async Task<bool> UpdateUserStatusAsync(Guid id, int status)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return false;

            user.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 獲取使用者列表
        /// </summary>
        public async Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(
            int page = 1,
            int pageSize = 20,
            string? searchKeyword = null)
        {
            var query = _context.Users
                .Include(u => u.Person)
                .Where(u => u.Status == 0);

            if (!string.IsNullOrEmpty(searchKeyword))
                query = query.Where(u => u.Email.Contains(searchKeyword) ||
                                       u.UserName.Contains(searchKeyword) ||
                                       (u.Person != null && u.Person.DisplayName != null && u.Person.DisplayName.Contains(searchKeyword)));

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                if (user.Person != null)
                {
                    userDtos.Add(new UserDto
                    {
                        UserId = user.UserId,
                        Role = user.Role,
                        UserName = user.UserName,
                        Email = user.Email,
                        CreateTime = user.CreateTime,
                        Status = user.Status,
                        Person = new PersonDto
                        {
                            PersonId = user.Person.PersonId,
                            UserId = user.Person.UserId,
                            DisplayName = user.Person.DisplayName,
                            Bio = user.Person.Bio,
                            AvatarPath = user.Person.AvatarPath,
                            IsPrivate = user.Person.IsPrivate
                        }
                    });
                }
            }

            return (userDtos, totalCount);
        }

        /// <summary>
        /// 檢查Email是否存在
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// 檢查使用者名稱是否存在
        /// </summary>
        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        /// <summary>
        /// 搜尋使用者
        /// </summary>
        public async Task<(List<UserDto> Items, int TotalCount)> SearchAsync(
            string? keyword = null,
            int page = 1,
            int pageSize = 20)
        {
            var (Users, TotalCount) = await GetUsersAsync(page, pageSize, keyword);
            return (Users, TotalCount);
        }

        /// <summary>
        /// 更新狀態
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            return await UpdateUserStatusAsync(id, status);
        }

        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(password + "salt")));
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}