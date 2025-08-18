using Matrix.Models;

namespace Matrix.Services
{
    /// <summary>
    /// 追蹤服務
    /// </summary>
#pragma warning disable CS9113 // Parameter is unread
    public class FollowService : IFollowService
#pragma warning restore CS9113
    {
        private readonly ApplicationDbContext _context;
        private const int TypeUser = 1;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        // === 你現在需要的功能：我追蹤誰 ===
        public async Task<(List<FollowDto> Following, int TotalCount)> GetFollowingAsync(
            Guid userId, int page = 1, int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            // 先取 total
            var baseQuery = _context.Follows.AsNoTracking()
                .Where(f => f.UserId == userId && f.Type == TypeUser);

            var total = await baseQuery.CountAsync();

            // Join 被追蹤者 Person（★ FollowedId = PersonId）
            var rows = await baseQuery
                .OrderByDescending(f => f.FollowTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.Persons.AsNoTracking(),
                    f => f.FollowedId,          // FollowedId = Persons.PersonId
                    p => p.PersonId,
                    (f, p) => new { f, p }
                )
                .ToListAsync();

            // 組成你的 FollowDto（Status 先固定 0；IsMutualFollow 暫不計算）
            var list = rows.Select(r => new FollowDto
            {
                FollowId = r.f.FollowId,
                FollowerId = r.f.UserId,            // 我（AspNetUsers.Id）
                FollowedId = r.f.FollowedId,        // 對方（Persons.PersonId）
                CreateTime = r.f.FollowTime,
                Status = 0,                     // DB 尚未有欄位 → 先固定正常
                Follower = null,                  // 目前頁面用不到可先省略（需要再補）
                Followed = new PersonDto
                {
                    PersonId = r.p.PersonId,
                    UserId = r.p.UserId,
                    DisplayName = r.p.DisplayName ?? "未知用戶",
                    AvatarPath = string.IsNullOrEmpty(r.p.AvatarPath)
                                  ? "/static/images/default_avatar.png"
                                  : r.p.AvatarPath,
                    //IsPublic = false              // Person 無此欄位 → 先預設
                },
                IsMutualFollow = false
            }).ToList();

            return (list, total);
        }

        // === 其餘方法暫不實作（佔位） ===
        public Task<bool> FollowUserAsync(Guid followerId, Guid followedId)
            => throw new NotImplementedException();

        public Task<bool> UnfollowUserAsync(Guid followerId, Guid followedId)
            => throw new NotImplementedException();

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
        {
            return await _context.Follows
                .AsNoTracking()
                .AnyAsync(f =>
                    f.UserId == followerId &&
                    f.FollowedId == followedId &&
                    f.Type == TypeUser // TypeUser = 1（追蹤使用者）
                );
        }
        

        public Task<(List<FollowDto> Followers, int TotalCount)> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
            => throw new NotImplementedException();

        public Task<int> GetFollowerCountAsync(Guid userId)
            => throw new NotImplementedException();

        public Task<int> GetFollowingCountAsync(Guid userId)
            => throw new NotImplementedException();

        public Task<List<Guid>> GetMutualFollowsAsync(Guid userId1, Guid userId2)
            => throw new NotImplementedException();

        public async Task<List<FollowUserSearchDto>> SearchUsersAsync(Guid currentPersonId, string keyword)
        {
            keyword = (keyword ?? string.Empty).Trim();
            if (keyword.Length == 0) return new List<FollowUserSearchDto>();

            return await _context.Persons
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p =>
                    p.PersonId != currentPersonId &&
                    (EF.Functions.Like(p.DisplayName ?? string.Empty, $"%{keyword}%") ||
                     EF.Functions.Like(p.User!.UserName, $"%{keyword}%")))
                .Select(p => new FollowUserSearchDto
                {
                    PersonId = p.PersonId,
                    DisplayName = p.DisplayName ?? "無名氏",
                    AvatarPath = string.IsNullOrEmpty(p.AvatarPath) ? "/static/img/cute.png" : p.AvatarPath,
                    // 關鍵：用 currentPersonId（我的 PersonId）對 Follows.UserId（你表裡存的是 PersonId）
                    IsFollowed = _context.Follows.Any(f =>
                        f.UserId == currentPersonId &&
                        f.FollowedId == p.PersonId &&
                        f.Type == 1)
                })
                .Take(10)
                .ToListAsync();
        }

        public async Task<bool> FollowAsync(Guid followerPersonId, Guid followedPersonId)
        {
            if (followerPersonId == followedPersonId) return false;

            var exist = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.UserId == followerPersonId
                            && f.FollowedId == followedPersonId
                            && f.Type == 1);
            if (exist) return true;

            _context.Follows.Add(new Follow
            {
                UserId = followerPersonId,
                FollowedId = followedPersonId,
                Type = 1,
                FollowTime = DateTime.UtcNow,
                User = null!
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowAsync(Guid followerPersonId, Guid followedPersonId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.UserId == followerPersonId
                                       && f.FollowedId == followedPersonId
                                       && f.Type == 1);

            if (follow == null) return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}