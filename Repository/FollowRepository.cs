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
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == followerId)
                .OrderByDescending(f => f.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Follow>> GetFollowersAsync(Guid followedId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == followedId)
                .OrderByDescending(f => f.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
        {
            return await _dbSet
                .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task<Follow?> GetFollowRelationAsync(Guid followerId, Guid followedId)
        {
            return await _dbSet
                .Include(f => f.Follower)
                .Include(f => f.Followed)
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task<int> CountFollowingAsync(Guid followerId)
        {
            return await _dbSet.CountAsync(f => f.FollowerId == followerId);
        }

        public async Task<int> CountFollowersAsync(Guid followedId)
        {
            return await _dbSet.CountAsync(f => f.FollowedId == followedId);
        }

        public async Task<IEnumerable<Follow>> GetMutualFollowsAsync(Guid userId)
        {
            // 取得相互追蹤的關係（用戶追蹤的人同時也追蹤了用戶）
            var following = await _dbSet
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            return await _dbSet
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == userId && 
                           _dbSet.Any(mutual => mutual.FollowerId == f.FollowedId && 
                                              mutual.FollowedId == userId))
                .ToListAsync();
        }
    }
}