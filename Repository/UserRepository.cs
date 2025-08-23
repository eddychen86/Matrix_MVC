using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 用戶資料存取實作
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(ApplicationDbContext context, IPasswordHasher<User> passwordHasher) : base(context)
        {
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserBasicDto>> GetAllWithUserAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(u => u.Status != 0 && u.Role == 0 && u.IsDelete == 0) // 過濾已刪除用戶
                .Select(u => new UserBasicDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    LastLoginTime = u.LastLoginTime
                })
                .ToListAsync();
        }

        public async Task<List<AdminDto>> GetAllWithAdminAsync(int pages = 1, int pageSize = 5)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.Status != 0 && u.Role > 0 && u.IsDelete == 0) // 過濾已刪除管理員
                .Skip((pages - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    DisplayName = u.Person != null ? u.Person.DisplayName : null,
                    LastLoginTime = u.LastLoginTime,
                    Role = u.Role,
                    Email = u.Email,
                    Status = u.Status,
                    IsDelete = u.IsDelete,
                    AvatarPath = u.Person != null ? u.Person.AvatarPath : null
                })
                .ToListAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithPersonAsync(Guid userId)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _dbSet
                // .AsNoTracking()      <== 驗證屬更新，不該加上禁止追蹤的功能
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null || string.IsNullOrEmpty(user.Password)) return false;

            // 判斷密碼是新格式還是舊格式
            // Identity V3 的雜湊值以 AQAAAA== 開頭
            bool isNewHash = user.Password.StartsWith("AQAAAA");

            if (isNewHash)
            {
                // 使用 Identity 的 PasswordHasher 來驗證新格式的密碼
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            else
            {
                // 使用舊的雜湊方法來驗證舊格式的密碼
                string oldHashedPassword = HashPasswordForMigration(password);
                if (user.Password == oldHashedPassword)
                {
                    // 驗證成功，但延後密碼升級到背景處理，避免阻塞認證
                    // 使用 Task.Run 在背景線程升級密碼，不阻塞當前請求
                    // _ = Task.Run(async () => await UpgradePasswordAsync(user.UserId, password));
                    // return true;

                    // 直接在當前請求中更新
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await UpdateAsync(user);
                    await SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 背景升級舊密碼格式，不阻塞主要認證流程
        /// </summary>
        private async Task UpgradePasswordAsync(Guid userId, string password)
        {
            try
            {
                var user = await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null && !user.Password.StartsWith("AQAAAA"))
                {
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await UpdateAsync(user);
                    await SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password upgrade failed for user {userId}: {ex.Message}");
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            // 檢查所有用戶（包括已刪除），因為已刪除用戶的用戶名仍應視為已被使用
            return await _dbSet.AsNoTracking().AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            // 檢查所有用戶（包括已刪除），因為已刪除用戶的郵箱仍應視為已被使用
            return await _dbSet.AsNoTracking().AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserName == identifier || u.Email == identifier);
        }

        /// <summary>
        /// 覆寫 GetByIdAsync 以包含 Person 資料
        /// </summary>
        public override async Task<User?> GetByIdAsync<TKey>(TKey id)
        {
            if (id is Guid userId)
            {
                return await GetUserWithPersonAsync(userId);
            }
            return await base.GetByIdAsync(id);
        }

        /// <summary>
        /// 僅用於遷移舊密碼的雜湊方法，不應用於新密碼
        /// </summary>
        private static string HashPasswordForMigration(string password)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(password + "salt")));
        }

        // === 軟刪除相關方法實作 ===

        /// <summary>
        /// 軟刪除用戶（設定 IsDelete = 1）
        /// </summary>
        public async Task<bool> SoftDeleteAsync(Guid userId)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.IsDelete = 1;
            await UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 恢復已刪除的用戶（設定 IsDelete = 0）
        /// </summary>
        public async Task<bool> RestoreAsync(Guid userId)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.IsDelete = 0;
            await UpdateAsync(user);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 取得所有未刪除的用戶
        /// </summary>
        public async Task<List<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.IsDelete == 0)
                .ToListAsync();
        }

        /// <summary>
        /// 取得所有已刪除的用戶
        /// </summary>
        public async Task<List<User>> GetDeletedUsersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.IsDelete == 1)
                .ToListAsync();
        }

        /// <summary>
        /// 取得未刪除的管理員列表（分頁）
        /// </summary>
        public async Task<List<AdminDto>> GetActiveAdminsAsync(int pages = 1, int pageSize = 5)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.Status != 0 && u.Role > 0 && u.IsDelete == 0)
                .Skip((pages - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    DisplayName = u.Person != null ? u.Person.DisplayName : null,
                    LastLoginTime = u.LastLoginTime,
                    Role = u.Role,
                    Email = u.Email,
                    Status = u.Status,
                    IsDelete = u.IsDelete,
                    AvatarPath = u.Person != null ? u.Person.AvatarPath : null
                })
                .ToListAsync();
        }

        /// <summary>
        /// 取得已刪除的管理員列表（分頁）
        /// </summary>
        public async Task<List<AdminDto>> GetDeletedAdminsAsync(int pages = 1, int pageSize = 5)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.Role > 0 && u.IsDelete == 1)
                .Skip((pages - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    DisplayName = u.Person != null ? u.Person.DisplayName : null,
                    LastLoginTime = u.LastLoginTime,
                    Role = u.Role,
                    Email = u.Email,
                    Status = u.Status,
                    IsDelete = u.IsDelete,
                    AvatarPath = u.Person != null ? u.Person.AvatarPath : null
                })
                .ToListAsync();
        }

        /// <summary>
        /// 檢查用戶是否已被軟刪除
        /// </summary>
        public async Task<bool> IsUserDeletedAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.UserId == userId && u.IsDelete == 1);
        }

        /// <summary>
        /// 根據用戶名取得未刪除的用戶
        /// </summary>
        public async Task<User?> GetActiveByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserName == username && u.IsDelete == 0);
        }

        /// <summary>
        /// 根據電子郵件取得未刪除的用戶
        /// </summary>
        public async Task<User?> GetActiveByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsDelete == 0);
        }

        /// <summary>
        /// 檢查用戶名是否在未刪除用戶中存在
        /// </summary>
        public async Task<bool> ActiveUsernameExistsAsync(string username)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.UserName == username && u.IsDelete == 0);
        }

        /// <summary>
        /// 檢查電子郵件是否在未刪除用戶中存在
        /// </summary>
        public async Task<bool> ActiveEmailExistsAsync(string email)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Email == email && u.IsDelete == 0);
        }
    }
}
