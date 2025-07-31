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
                .Include(f => f.Requester)
                .Include(f => f.Recipient)
                .Where(f => (f.UserId == userId || f.FriendId == userId) && 
                           f.Status == FriendshipStatus.Accepted)
                .OrderByDescending(f => f.RequestDate) // Model 中沒有 ResponseDate，暫用 RequestDate
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetPendingFriendRequestsAsync(Guid userId)
        {
            return await _dbSet
                .Include(f => f.Requester)
                .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
                .OrderByDescending(f => f.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetSentFriendRequestsAsync(Guid userId)
        {
            return await _dbSet
                .Include(f => f.Recipient)
                .Where(f => f.UserId == userId && f.Status == FriendshipStatus.Pending)
                .OrderByDescending(f => f.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
        {
            return await _dbSet
                .AnyAsync(f => ((f.UserId == userId1 && f.FriendId == userId2) ||
                               (f.UserId == userId2 && f.FriendId == userId1)) &&
                              f.Status == FriendshipStatus.Accepted);
        }

        public async Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2)
        {
            return await _dbSet
                .Include(f => f.Requester)
                .Include(f => f.Recipient)
                .FirstOrDefaultAsync(f => (f.UserId == userId1 && f.FriendId == userId2) ||
                                         (f.UserId == userId2 && f.FriendId == userId1));
        }

        public async Task<int> CountFriendsAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(f => (f.UserId == userId || f.FriendId == userId) && 
                                f.Status == FriendshipStatus.Accepted);
        }

        public async Task<int> CountPendingRequestsAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending);
        }

        public async Task UpdateFriendshipStatusAsync(Guid friendshipId, FriendshipStatus status)
        {
            var friendship = await _dbSet.FindAsync(friendshipId);
            if (friendship != null)
            {
                friendship.Status = status;
                // friendship.ResponseDate = DateTime.Now; // Model 中無此欄位
                await _context.SaveChangesAsync();
            }
        }
    }
}
