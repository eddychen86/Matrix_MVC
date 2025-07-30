using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 好友關係資料存取實作
    /// </summary>
    public class FriendshipRepository : BaseRepository<Friendship>, IFriendshipRepository
    {
        public FriendshipRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Friendship>> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(f => f.RequesterUser)
                .Include(f => f.AddresseeUser)
                .Where(f => (f.RequesterUserId == userId || f.AddresseeUserId == userId) && 
                           f.Status == 1) // 假設 Status 1 表示已接受
                .OrderByDescending(f => f.ResponseDate ?? f.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetPendingFriendRequestsAsync(Guid userId)
        {
            return await _dbSet
                .Include(f => f.RequesterUser)
                .Where(f => f.AddresseeUserId == userId && f.Status == 0) // 假設 Status 0 表示待處理
                .OrderByDescending(f => f.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetSentFriendRequestsAsync(Guid userId)
        {
            return await _dbSet
                .Include(f => f.AddresseeUser)
                .Where(f => f.RequesterUserId == userId && f.Status == 0)
                .OrderByDescending(f => f.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
        {
            return await _dbSet
                .AnyAsync(f => ((f.RequesterUserId == userId1 && f.AddresseeUserId == userId2) ||
                               (f.RequesterUserId == userId2 && f.AddresseeUserId == userId1)) &&
                              f.Status == 1);
        }

        public async Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2)
        {
            return await _dbSet
                .Include(f => f.RequesterUser)
                .Include(f => f.AddresseeUser)
                .FirstOrDefaultAsync(f => (f.RequesterUserId == userId1 && f.AddresseeUserId == userId2) ||
                                         (f.RequesterUserId == userId2 && f.AddresseeUserId == userId1));
        }

        public async Task<int> CountFriendsAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(f => (f.RequesterUserId == userId || f.AddresseeUserId == userId) && 
                                f.Status == 1);
        }

        public async Task<int> CountPendingRequestsAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(f => f.AddresseeUserId == userId && f.Status == 0);
        }

        public async Task UpdateFriendshipStatusAsync(Guid friendshipId, int status)
        {
            var friendship = await _dbSet.FindAsync(friendshipId);
            if (friendship != null)
            {
                friendship.Status = status;
                friendship.ResponseDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}