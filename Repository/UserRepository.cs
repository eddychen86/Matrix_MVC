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
                .Where(u => u.Status != 0 && u.Role == 0) // 在資料庫層篩選
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
                .Where(u => u.Status != 0 && u.Role > 0) // 在資料庫層篩選
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
            return await _dbSet.AsNoTracking().AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
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
    }
}
