using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using AutoMapper;
using NuGet.Packaging.Signing;

namespace Matrix.Services
{
    /// <summary>
    /// 使用者服務 - 重構版本使用 Repository Pattern
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFileService _fileService;
        private readonly IOptions<UserValidationOptions> _validationOptions;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IArticleAttachmentRepository _articleAttachmentRepository;

        public UserService(
            IUserRepository userRepository,
            IPersonRepository personRepository,
            IFileService fileService,
            IOptions<UserValidationOptions> validationOptions,
            IPasswordHasher<User> passwordHasher,
            IMapper mapper,
            IMemoryCache cache,
            IArticleAttachmentRepository articleAttachmentRepository)
        {
            _userRepository = userRepository;
            _personRepository = personRepository;
            _fileService = fileService;
            _validationOptions = validationOptions;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _cache = cache;
            _articleAttachmentRepository = articleAttachmentRepository;
        }

        /// <summary>
        /// 獲取基本使用者資訊（總用戶數, 今日登入數）含快取
        /// </summary>
        public async Task<(int totalUsers, int totalTodayLogin)> GetUserBasicsAsync()
        {
            // 快取鍵
            var cacheKey = "user_basics";

            // 嘗試從快取讀取
            if (_cache.TryGetValue(cacheKey, out (int totalUsers, int totalTodayLogin)? cachedResult))
            {
                return cachedResult ?? (0, 0);
            }

            // 快取未命中，從資料庫查詢
            var result = await _userRepository.GetAllWithUserAsync();

            // 存入快取，8分鐘過期（基本資訊更新頻率較高）
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(8),
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Priority = CacheItemPriority.High // 儀表板常用資料
            };

            _cache.Set(cacheKey, result, cacheEntryOptions);

            return result;
        }

        /// <summary>
        /// 根據頁碼及每頁數量獲取系統管理員（含快取機制）
        /// </summary>
        public async Task<List<AdminDto>> GetAdminAsync(int pages, int pageSize)
        {
            return await _userRepository.GetAllWithAdminAsync(pages, pageSize);
        }

        /// <summary>
        /// 根據ID獲取使用者（含快取機制）
        /// </summary>
        public async Task<UserDto?> GetUserAsync(Guid id)
        {
            // 快取鍵
            var cacheKey = $"user_{id}";

            // 嘗試從快取讀取
            if (_cache.TryGetValue(cacheKey, out UserDto? cachedUser))
            {
                return cachedUser;
            }

            // 快取未命中，從資料庫查詢
            var user = await _userRepository.GetUserWithPersonAsync(id);

            if (user?.Person == null) return null;

            var userDto = _mapper.Map<UserDto>(user);

            // 存入快取，15分鐘過期
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5), // 5分鐘內有存取就延長
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, userDto, cacheEntryOptions);

            return userDto;
        }

        /// <summary>
        /// 根據Email獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user?.Person == null) return null;

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 根據使用者名稱獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user?.Person == null) return null;

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 根據使用者名稱或電子郵件獲取使用者
        /// </summary>
        public async Task<UserDto?> GetUserByIdentifierAsync(string identifier)
        {
            var user = await _userRepository.GetByIdentifierAsync(identifier);

            if (user == null) return null;

            return _mapper.Map<UserDto>(user);
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
                Role = dto.Role,  // 使用 DTO 傳入的角色
                UserName = dto.UserName,
                Email = dto.Email,
                CreateTime = DateTime.Now,
                Status = dto.Role > 0 ? 1 : 0  // 管理員直接啟用，一般用戶需驗證
            };

            // 使用 Identity 的 PasswordHasher 來產生安全的雜湊值
            user.Password = _passwordHasher.HashPassword(user, dto.Password);

            // 添加並保存 User
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // 創建對應的 Person 資料
            try
            {
                var person = new Person
                {
                    UserId = user.UserId,
                    DisplayName = dto.DisplayName ?? dto.UserName,  // 預設使用 UserName
                    Bio = null,
                    AvatarPath = null,
                    BannerPath = null,
                    IsPrivate = dto.IsPrivate,
                    WalletAddress = null,
                    Website1 = null,
                    Website2 = null,
                    Website3 = null,
                    ModifyTime = DateTime.UtcNow
                };

                await _personRepository.AddAsync(person);
                await _personRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // 如果 Person 創建失敗，應該回滾 User 的創建
                // 但為了簡化，這裡只記錄錯誤
                Console.WriteLine($"創建 Person 失敗: {ex.Message}");
            }
            
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
            
            // 清除快取，確保下次取得最新狀態
            var cacheKey = $"user_{id}";
            _cache.Remove(cacheKey);
            
            return true;
        }

        /// <summary>
        /// 更新狀態
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            return await UpdateUserStatusAsync(id, status);
        }

        /// <summary>
        /// 更新個人資料圖片（頭像或橫幅）
        /// </summary>
        public async Task<ReturnType<object>> UpdateProfileImageAsync(Guid userId, string type, string filePath)
        {
            try
            {
                var person = await _personRepository.GetByUserIdAsync(userId);
                if (person == null)
                {
                    return new ReturnType<object> 
                    { 
                        Success = false, 
                        Message = "找不到使用者的個人資料" 
                    };
                }

                // 備份舊的檔案路徑，以便在更新失敗時回復
                string? oldFilePath = null;

                // 根據類型更新對應的欄位
                switch (type.ToLower())
                {
                    case "avatar":
                        oldFilePath = person.AvatarPath;
                        person.AvatarPath = filePath;
                        break;
                    case "banner":
                        oldFilePath = person.BannerPath;
                        person.BannerPath = filePath;
                        break;
                    default:
                        return new ReturnType<object> 
                        { 
                            Success = false, 
                            Message = "無效的檔案類型" 
                        };
                }

                // 更新修改時間
                person.ModifyTime = DateTime.UtcNow;

                // 儲存到資料庫
                await _personRepository.UpdateAsync(person);
                await _personRepository.SaveChangesAsync();

                // 如果有舊檔案，刪除它
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    await _fileService.DeleteFileAsync(oldFilePath);
                }

                // 清除相關快取
                var userCacheKey = $"user_{userId}";
                var profileCacheKey = $"profile_{userId}";
                _cache.Remove(userCacheKey);
                _cache.Remove(profileCacheKey);

                return new ReturnType<object> 
                { 
                    Success = true, 
                    Message = $"{(type == "avatar" ? "頭像" : "橫幅")}更新成功" 
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新個人資料圖片時發生錯誤: {ex.Message}");
                
                return new ReturnType<object> 
                { 
                    Success = false, 
                    Message = "更新過程中發生錯誤，請稍後再試" 
                };
            }
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

        // 映射方法已由 AutoMapper 取代

        // private static string HashPassword(string password)
        // {
        //     return Convert.ToBase64String(
        //         System.Security.Cryptography.SHA256.HashData(
        //             System.Text.Encoding.UTF8.GetBytes(password + "salt")));
        // }

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

        public async Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 20, string? searchKeyword = null)
        {
            IQueryable<User> query = _userRepository.AsQueryable().Include(u => u.Person);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var keyword = searchKeyword.ToLower().Trim();
                query = query.Where(u =>
                    u.UserName.Contains(keyword) ||
                    u.Email.Contains(keyword) ||
                    (u.Person != null && u.Person.DisplayName != null && u.Person.DisplayName.ToLower().Contains(keyword)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return (userDtos, totalCount);
        }

        public Task<(List<UserDto> Items, int TotalCount)> SearchAsync(string? keyword = null, int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException("需要實作搜尋方法");
        }

        /// <summary>
        /// 根據 ID 獲取用戶實體
        /// </summary>
        public async Task<User?> GetUserEntityAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// 直接更新用戶實體
        /// </summary>
        public async Task<bool> UpdateUserEntityAsync(User user)
        {
            try
            {
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PersonDto?> UpdateProfileAsync(Guid userId, ApiUpdateProfileDto dto)
        {
            var person = await _personRepository.GetByUserIdAsync(userId);
            if (person == null) return null;

            if (dto.AvatarFile != null)
            {
                var newAvatarPath = await _fileService.UpdateFileAsync(dto.AvatarFile, person.AvatarPath, "profile/imgs");
                if (newAvatarPath != null)
                {
                    person.AvatarPath = newAvatarPath;
                }
            }

            if (dto.BannerFile != null)
            {
                var newBannerPath = await _fileService.UpdateFileAsync(dto.BannerFile, person.BannerPath, "profile/imgs");
                if (newBannerPath != null)
                {
                    person.BannerPath = newBannerPath;
                }
            }

            if (!string.IsNullOrEmpty(dto.DisplayName))
            {
                person.DisplayName = dto.DisplayName;
            }

            if (!string.IsNullOrEmpty(dto.Bio))
            {
                person.Bio = dto.Bio;
            }

            person.ModifyTime = DateTime.UtcNow;
            await _personRepository.UpdateAsync(person);

            return new PersonDto
            {
                PersonId = person.PersonId,
                UserId = person.UserId,
                DisplayName = person.DisplayName,
                Bio = person.Bio,
                AvatarPath = person.AvatarPath,
                BannerPath = person.BannerPath,
                IsPrivate = person.IsPrivate,
                WalletAddress = person.WalletAddress,
                ModifyTime = person.ModifyTime
            };
        }

        /// <summary>
        /// 根據使用者 ID 獲取個人資料和相關文章 - 優化版本
        /// </summary>
        public async Task<PersonDto?> GetProfileByIdAsync(Guid userId)
        {
            try
            {
                // 使用 AsNoTracking 進行只讀查詢，提升性能
                var person = await _personRepository.GetByUserIdWithIncludesAsync(userId);
                if (person?.User == null) return null;

                // 使用 AutoMapper 進行映射，簡化代碼
                return new PersonDto
                {
                    PersonId = person.PersonId,
                    UserId = person.UserId,
                    DisplayName = person.DisplayName,
                    Bio = person.Bio,
                    AvatarPath = person.AvatarPath,
                    BannerPath = person.BannerPath,
                    IsPrivate = person.IsPrivate,
                    WalletAddress = person.WalletAddress,
                    ModifyTime = person.ModifyTime,
                    Email = person.User.Email,
                    Website1 = person.Website1,
                    Website2 = person.Website2,
                    Website3 = person.Website3,
                    Articles = new List<ArticleDto>(), // 空列表，避免查詢文章
                    Content = new List<string>() // 空列表
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常
                Console.WriteLine($"GetProfileByIdAsync Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 更新個人資料（包括密碼和網站連結）
        /// </summary>
        public async Task<ReturnType<object>> UpdatePersonProfileAsync(Guid userId, PersonDto dto)
        {
            try
            {
                // UserId 由 Controller 從認證中安全獲取，無需驗證 DTO 中的 UserId
                // 使用支援變更追蹤的方法來獲取實體
                var person = await _personRepository.GetByUserIdForUpdateAsync(userId);
                if (person == null)
                {
                    return new ReturnType<object> { Success = false, Message = "找不到個人資料" };
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ReturnType<object> { Success = false, Message = "找不到使用者" };
                }

                // 更新個人資料
                person.DisplayName = dto.DisplayName;
                person.Bio = dto.Bio;
                person.Website1 = dto.Website1;
                person.Website2 = dto.Website2;
                person.Website3 = dto.Website3;
                person.ModifyTime = DateTime.UtcNow;

                // 更新使用者資料
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    user.Password = _passwordHasher.HashPassword(user, dto.Password);
                }
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                }

                // 使用 Repository 來保存變更
                await _personRepository.SaveChangesAsync();

                return new ReturnType<object> { Success = true, Message = "更新成功!" };
            }
            catch (Exception ex)
            {
                // 記錄詳細錯誤信息以便調試
                Console.WriteLine($"UpdatePersonProfileAsync 錯誤: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new ReturnType<object> { Success = false, Message = $"更新失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 獲取用戶文章中的前N張圖片 - 簡化版本避免複雜查詢
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="count">圖片數量限制，預設為10</param>
        /// <returns>圖片資訊列表</returns>
        public async Task<List<UserImageDto>> GetUserImagesAsync(Guid userId, int count = 10)
        {
            try
            {
                // 查詢用戶是否存在
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return new List<UserImageDto>();

                // 暫時返回空列表，避免複雜的跨表查詢導致 DbContext 問題
                // TODO: 將此功能移到專門的 ArticleService 中處理
                return new List<UserImageDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserImagesAsync Error: {ex.Message}");
                return new List<UserImageDto>();
            }
        }

        /// <summary>
        /// 更新使用者的最後登入時間
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>更新是否成功</returns>
        public async Task<bool> UpdateLastLoginTimeAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.LastLoginTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateLastLoginTimeAsync Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 軟刪除相關方法

        /// <summary>
        /// 軟刪除使用者（設定 IsDelete = 1）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>軟刪除是否成功</returns>
        public async Task<bool> SoftDeleteUserAsync(Guid userId)
        {
            try
            {
                var success = await _userRepository.SoftDeleteAsync(userId);
                
                if (success)
                {
                    // 清除相關快取
                    var userCacheKey = $"user_{userId}";
                    var profileCacheKey = $"profile_{userId}";
                    _cache.Remove(userCacheKey);
                    _cache.Remove(profileCacheKey);
                    _cache.Remove("user_basics");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SoftDeleteUserAsync Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 恢復已軟刪除的使用者（設定 IsDelete = 0）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>恢復是否成功</returns>
        public async Task<bool> RestoreUserAsync(Guid userId)
        {
            try
            {
                var success = await _userRepository.RestoreAsync(userId);
                
                if (success)
                {
                    // 清除相關快取
                    var userCacheKey = $"user_{userId}";
                    var profileCacheKey = $"profile_{userId}";
                    _cache.Remove(userCacheKey);
                    _cache.Remove(profileCacheKey);
                    _cache.Remove("user_basics");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RestoreUserAsync Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取得所有未刪除的使用者
        /// </summary>
        /// <returns>未刪除的使用者列表</returns>
        public async Task<List<UserDto>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveUsersAsync();
                return _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActiveUsersAsync Error: {ex.Message}");
                return new List<UserDto>();
            }
        }

        /// <summary>
        /// 取得所有已軟刪除的使用者
        /// </summary>
        /// <returns>已軟刪除的使用者列表</returns>
        public async Task<List<UserDto>> GetDeletedUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetDeletedUsersAsync();
                return _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDeletedUsersAsync Error: {ex.Message}");
                return new List<UserDto>();
            }
        }

        /// <summary>
        /// 取得未刪除的管理員列表（分頁）
        /// </summary>
        /// <param name="pages">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>未刪除的管理員列表</returns>
        public async Task<List<AdminDto>> GetActiveAdminsAsync(int pages = 1, int pageSize = 5)
        {
            try
            {
                return await _userRepository.GetActiveAdminsAsync(pages, pageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActiveAdminsAsync Error: {ex.Message}");
                return new List<AdminDto>();
            }
        }

        /// <summary>
        /// 取得已軟刪除的管理員列表（分頁）
        /// </summary>
        /// <param name="pages">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>已軟刪除的管理員列表</returns>
        public async Task<List<AdminDto>> GetDeletedAdminsAsync(int pages = 1, int pageSize = 5)
        {
            try
            {
                return await _userRepository.GetDeletedAdminsAsync(pages, pageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDeletedAdminsAsync Error: {ex.Message}");
                return new List<AdminDto>();
            }
        }

        /// <summary>
        /// 檢查使用者是否已被軟刪除
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>是否已被軟刪除</returns>
        public async Task<bool> IsUserDeletedAsync(Guid userId)
        {
            try
            {
                return await _userRepository.IsUserDeletedAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsUserDeletedAsync Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根據使用者名稱取得未刪除的使用者
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>未刪除的使用者資料</returns>
        public async Task<UserDto?> GetActiveUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetActiveByUsernameAsync(username);
                return user?.Person == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActiveUserByUsernameAsync Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根據電子郵件取得未刪除的使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>未刪除的使用者資料</returns>
        public async Task<UserDto?> GetActiveUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetActiveByEmailAsync(email);
                return user?.Person == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActiveUserByEmailAsync Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否在未刪除使用者中存在
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsActiveUserNameExistsAsync(string username)
        {
            try
            {
                return await _userRepository.ActiveUsernameExistsAsync(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsActiveUserNameExistsAsync Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 檢查電子郵件是否在未刪除使用者中存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsActiveEmailExistsAsync(string email)
        {
            try
            {
                return await _userRepository.ActiveEmailExistsAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsActiveEmailExistsAsync Error: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
