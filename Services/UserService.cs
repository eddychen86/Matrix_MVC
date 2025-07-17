using System;
using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;

namespace Matrix.Services;

/// <summary>
/// 使用者服務類別
/// 
/// 此服務負責處理與使用者相關的業務邏輯，包括：
/// - 使用者註冊和登入驗證
/// - 使用者資料的 CRUD 操作
/// - 密碼重置和安全性管理
/// - 使用者狀態管理
/// - 使用者權限驗證
/// 
/// 注意事項：
/// - 所有方法都應該包含適當的錯誤處理
/// - 敏感資料（如密碼）應該經過加密處理
/// - 包含完整的資料驗證邏輯
/// - 支援非同步操作以提高效能
/// </summary>
public class UserService
{
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// 建構函式
    /// 用途：初始化服務並注入資料庫上下文
    /// </summary>
    /// <param name="context">資料庫上下文</param>
    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 根據 ID 獲取使用者資料
    /// 用途：查詢特定使用者的完整資料
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null) return null;
            
            return new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                UserName = user.UserName,
                Email = user.Email,
                Country = user.Country,
                Gender = user.Gender,
                CreateTime = user.CreateTime,
                LastLoginTime = user.LastLoginTime,
                Status = user.Status,
                Person = user.Person != null ? new PersonDto
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
                } : null
            };
        }
        catch (Exception ex)
        {
            // 記錄錯誤日誌
            Console.WriteLine($"獲取使用者資料時發生錯誤: {ex.Message}");
            throw new Exception("獲取使用者資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 根據電子郵件獲取使用者資料
    /// 用途：通過電子郵件查詢使用者資料
    /// </summary>
    /// <param name="email">使用者電子郵件</param>
    /// <returns>使用者資料傳輸物件，如果不存在則返回 null</returns>
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null) return null;
            
            return new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                UserName = user.UserName,
                Email = user.Email,
                Country = user.Country,
                Gender = user.Gender,
                CreateTime = user.CreateTime,
                LastLoginTime = user.LastLoginTime,
                Status = user.Status,
                Person = user.Person != null ? new PersonDto
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
                } : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"通過電子郵件獲取使用者資料時發生錯誤: {ex.Message}");
            throw new Exception("獲取使用者資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 建立新使用者
    /// 用途：註冊新使用者並建立對應的個人資料
    /// </summary>
    /// <param name="dto">建立使用者的資料傳輸物件</param>
    /// <returns>是否建立成功</returns>
    public async Task<bool> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            // 驗證輸入資料
            if (!dto.IsValid)
            {
                throw new ArgumentException("輸入資料無效");
            }
            
            // 檢查電子郵件是否已存在
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            
            if (existingUser != null)
            {
                throw new InvalidOperationException("此電子郵件已被使用");
            }
            
            // 檢查使用者名稱是否已存在
            var existingUserName = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            
            if (existingUserName != null)
            {
                throw new InvalidOperationException("此使用者名稱已被使用");
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 建立使用者實體
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Role = 0, // 預設為一般使用者
                    UserName = dto.UserName,
                    Email = dto.Email,
                    Password = HashPassword(dto.Password), // 密碼加密
                    Country = dto.Country,
                    Gender = dto.Gender,
                    CreateTime = DateTime.Now,
                    Status = 0, // 預設為正常狀態
                    Person = null! // 稍後會設定
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                // 建立對應的個人資料
                var person = new Person
                {
                    PersonId = Guid.NewGuid(),
                    UserId = user.UserId,
                    DisplayName = dto.DisplayName,
                    IsPrivate = dto.IsPrivate,
                    User = user
                };
                
                // 設定使用者的 Person 參照
                user.Person = person;
                
                _context.Persons.Add(person);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"建立使用者時發生錯誤: {ex.Message}");
            throw new Exception("建立使用者失敗", ex);
        }
    }
    
    /// <summary>
    /// 更新使用者資料
    /// 用途：更新現有使用者的資料
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="dto">更新使用者的資料傳輸物件</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            // 驗證輸入資料
            if (!dto.IsValid)
            {
                throw new ArgumentException("輸入資料無效");
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null)
            {
                throw new InvalidOperationException("找不到指定的使用者");
            }
            
            // 如果要更新電子郵件，檢查是否已存在
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.UserId != id);
                
                if (existingUser != null)
                {
                    throw new InvalidOperationException("此電子郵件已被使用");
                }
            }
            
            // 如果要更新使用者名稱，檢查是否已存在
            if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
            {
                var existingUserName = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.UserId != id);
                
                if (existingUserName != null)
                {
                    throw new InvalidOperationException("此使用者名稱已被使用");
                }
            }
            
            // 更新使用者資料
            if (!string.IsNullOrEmpty(dto.UserName))
                user.UserName = dto.UserName;
            
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;
            
            if (!string.IsNullOrEmpty(dto.Country))
                user.Country = dto.Country;
            
            if (dto.Gender.HasValue)
                user.Gender = dto.Gender.Value;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新使用者資料時發生錯誤: {ex.Message}");
            throw new Exception("更新使用者資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 刪除使用者
    /// 用途：軟刪除使用者（設定狀態為已刪除）
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null)
            {
                throw new InvalidOperationException("找不到指定的使用者");
            }
            
            // 軟刪除：設定狀態為已刪除
            user.Status = 2; // 2 = 已刪除狀態
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"刪除使用者時發生錯誤: {ex.Message}");
            throw new Exception("刪除使用者失敗", ex);
        }
    }
    
    /// <summary>
    /// 驗證使用者登入
    /// 用途：驗證使用者的電子郵件和密碼
    /// </summary>
    /// <param name="email">電子郵件</param>
    /// <param name="password">密碼</param>
    /// <returns>是否驗證成功</returns>
    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Status == 0);
            
            if (user == null)
            {
                return false;
            }
            
            // 驗證密碼
            bool isPasswordValid = VerifyPassword(password, user.Password);
            
            if (isPasswordValid)
            {
                // 更新最後登入時間
                user.LastLoginTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            return isPasswordValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"驗證使用者登入時發生錯誤: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 重設使用者密碼
    /// 用途：重設使用者的密碼
    /// </summary>
    /// <param name="email">使用者電子郵件</param>
    /// <param name="newPassword">新密碼</param>
    /// <returns>是否重設成功</returns>
    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Status == 0);
            
            if (user == null)
            {
                throw new InvalidOperationException("找不到指定的使用者");
            }
            
            // 驗證新密碼格式
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                throw new ArgumentException("新密碼長度必須至少 6 個字元");
            }
            
            // 更新密碼
            user.Password = HashPassword(newPassword);
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"重設密碼時發生錯誤: {ex.Message}");
            throw new Exception("重設密碼失敗", ex);
        }
    }
    
    /// <summary>
    /// 更新使用者狀態
    /// 用途：更新使用者的帳戶狀態
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="status">新狀態 (0=正常, 1=停用, 2=封禁)</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateUserStatusAsync(Guid id, int status)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null)
            {
                throw new InvalidOperationException("找不到指定的使用者");
            }
            
            if (status < 0 || status > 2)
            {
                throw new ArgumentException("狀態值無效");
            }
            
            user.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新使用者狀態時發生錯誤: {ex.Message}");
            throw new Exception("更新使用者狀態失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取使用者列表
    /// 用途：分頁查詢使用者列表
    /// </summary>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁數量</param>
    /// <param name="searchKeyword">搜尋關鍵字</param>
    /// <returns>使用者列表和總數</returns>
    public async Task<(List<UserDto> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 20, string? searchKeyword = null)
    {
        try
        {
            var query = _context.Users.Include(u => u.Person).AsQueryable();
            
            // 如果有搜尋關鍵字，進行搜尋
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(u => u.UserName.Contains(searchKeyword) || 
                                       u.Email.Contains(searchKeyword) ||
                                       (u.Person != null && u.Person.DisplayName != null && u.Person.DisplayName.Contains(searchKeyword)));
            }
            
            // 只查詢非已刪除的使用者
            query = query.Where(u => u.Status != 2);
            
            var totalCount = await query.CountAsync();
            
            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Role = u.Role,
                    UserName = u.UserName,
                    Email = u.Email,
                    Country = u.Country,
                    Gender = u.Gender,
                    CreateTime = u.CreateTime,
                    LastLoginTime = u.LastLoginTime,
                    Status = u.Status,
                    Person = u.Person != null ? new PersonDto
                    {
                        PersonId = u.Person.PersonId,
                        UserId = u.Person.UserId,
                        DisplayName = u.Person.DisplayName,
                        Bio = u.Person.Bio,
                        AvatarPath = u.Person.AvatarPath,
                        BannerPath = u.Person.BannerPath,
                        ExternalUrl = u.Person.ExternalUrl,
                        IsPrivate = u.Person.IsPrivate,
                        WalletAddress = u.Person.WalletAddress,
                        ModifyTime = u.Person.ModifyTime
                    } : null
                })
                .ToListAsync();
            
            return (users, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取使用者列表時發生錯誤: {ex.Message}");
            throw new Exception("獲取使用者列表失敗", ex);
        }
    }
    
    /// <summary>
    /// 檢查電子郵件是否已存在
    /// 用途：註冊前檢查電子郵件的唯一性
    /// </summary>
    /// <param name="email">電子郵件</param>
    /// <returns>是否已存在</returns>
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"檢查電子郵件是否存在時發生錯誤: {ex.Message}");
            throw new Exception("檢查電子郵件失敗", ex);
        }
    }
    
    /// <summary>
    /// 檢查使用者名稱是否已存在
    /// 用途：註冊前檢查使用者名稱的唯一性
    /// </summary>
    /// <param name="userName">使用者名稱</param>
    /// <returns>是否已存在</returns>
    public async Task<bool> IsUserNameExistsAsync(string userName)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"檢查使用者名稱是否存在時發生錯誤: {ex.Message}");
            throw new Exception("檢查使用者名稱失敗", ex);
        }
    }
    
    /// <summary>
    /// 密碼加密
    /// 用途：對密碼進行雜湊加密
    /// </summary>
    /// <param name="password">原始密碼</param>
    /// <returns>加密後的密碼</returns>
    private string HashPassword(string password)
    {
        // 這裡應該使用適當的密碼雜湊演算法，如 BCrypt
        // 為了簡化，這裡使用 SHA256（實際專案中應使用更安全的方法）
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "salt"));
        return Convert.ToBase64String(hashedBytes);
    }
    
    /// <summary>
    /// 驗證密碼
    /// 用途：驗證輸入的密碼是否正確
    /// </summary>
    /// <param name="password">輸入的密碼</param>
    /// <param name="hashedPassword">儲存的加密密碼</param>
    /// <returns>是否驗證成功</returns>
    private bool VerifyPassword(string password, string hashedPassword)
    {
        string hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }
}
