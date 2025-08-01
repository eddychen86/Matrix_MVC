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
                    // 驗證成功，立即將密碼升級為新格式並儲存
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await UpdateAsync(user);
                    await SaveChangesAsync();
                    return true;
                }
            }

            return false;

            // // 舊的 BCrypt 驗證密碼 (已棄用)
            // return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _dbSet
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
