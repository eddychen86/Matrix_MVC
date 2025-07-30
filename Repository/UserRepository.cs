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
        public UserRepository(ApplicationDbContext context) : base(context) { }

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

            if (user == null) return false;

            // 使用 BCrypt 驗證密碼
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
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
    }
}