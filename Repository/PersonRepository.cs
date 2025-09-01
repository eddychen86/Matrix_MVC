using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 個人資料存取實作
    /// </summary>
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        public PersonRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Person?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<Person>> SearchByDisplayNameAsync(string displayName)
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.DisplayName != null && p.DisplayName.ToLower().Contains(displayName.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Person>> GetPublicPersonsAsync(int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.IsPrivate == 0)
                .OrderByDescending(p => p.ModifyTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Person?> GetByWalletAddressAsync(string walletAddress)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.WalletAddress == walletAddress);
        }

        public async Task<Person?> GetByUserIdWithIncludesAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Person?> GetByUserIdForUpdateAsync(Guid userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<Person>> GetUserIdsByPersonIds(IEnumerable<Guid> personIds)
        {
            // 使用 Where 和 Contains 來一次性查詢所有 PersonId 在列表中的 Person 物件
            return await _dbSet.Where(p => personIds.Contains(p.PersonId))
                               .ToListAsync();
        }
    }
}