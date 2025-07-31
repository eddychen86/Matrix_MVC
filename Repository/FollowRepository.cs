using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 追蹤關係資料存取實作
    /// </summary>
    public class FollowRepository : BaseRepository<Follow>, IFollowRepository
    {
        public FollowRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Follow>> GetFollowingAsync(Guid followerId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(f => f.User) // 修正: Followed -> User (代表追蹤者)
                .Where(f => f.UserId == followerId) // 修正: FollowerId -> UserId
                .OrderByDescending(f => f.FollowTime) // 修正: CreateTime -> FollowTime
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Follow>> GetFollowersAsync(Guid followedId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(f => f.User) // 修正: Follower -> User (代表追蹤者)
                .Where(f => f.FollowedId == followedId)
                .OrderByDescending(f => f.FollowTime) // 修正: CreateTime -> FollowTime
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
        {
            return await _dbSet
                .AnyAsync(f => f.UserId == followerId && f.FollowedId == followedId); // 修正: FollowerId -> UserId
        }

        public async Task<Follow?> GetFollowRelationAsync(Guid followerId, Guid followedId)
        {
            return await _dbSet
                .Where(f => f.UserId == followerId && f.FollowedId == followedId) // 修正: FollowerId -> UserId
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountFollowingAsync(Guid followerId)
        {
            return await _dbSet.CountAsync(f => f.UserId == followerId); // 修正: FollowerId -> UserId
        }

        public async Task<int> CountFollowersAsync(Guid followedId)
        {
            return await _dbSet.CountAsync(f => f.FollowedId == followedId);
        }

        public async Task<IEnumerable<Follow>> GetMutualFollowsAsync(Guid userId)
        {
            // 取得用戶追蹤的人的ID
            var followingIds = await _dbSet
                .Where(f => f.UserId == userId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            if (!followingIds.Any()) return Enumerable.Empty<Follow>();

            // 找出這些人中，也追蹤了用戶的記錄
            return await _dbSet
                .Where(f => followingIds.Contains(f.UserId) && f.FollowedId == userId)
                .ToListAsync();
        }
    }
}
