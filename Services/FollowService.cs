namespace Matrix.Services
{ 
using Matrix.Services.Interfaces;

    /// <summary>
    /// 追蹤服務
    /// </summary>
    #pragma warning disable CS9113 // Parameter is unread
    public class FollowService(ApplicationDbContext _context) : IFollowService
    #pragma warning restore CS9113
    {
        
        public Task<bool> FollowUserAsync(Guid followerId, Guid followedId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> UnfollowUserAsync(Guid followerId, Guid followedId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<(List<FollowDto> Followers, int TotalCount)> GetFollowersAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            return Task.FromException<(List<FollowDto>, int)>(new NotImplementedException());
        }

        public Task<(List<FollowDto> Following, int TotalCount)> GetFollowingAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            return Task.FromException<(List<FollowDto>, int)>(new NotImplementedException());
        }

    /// <summary>
    /// 被追蹤數：有多少人追這個 userId（依你的模型：Type == 1 = 使用者追蹤）
    /// </summary>
    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        if (userId == Guid.Empty) return 0;

        return await _context.Follows
            .AsNoTracking()
            .CountAsync(f => f.Type == 1 && f.FollowedId == userId);
    }

    /// <summary>
    /// 追蹤數：這個 userId 追了多少人（Type == 1）
    /// </summary>
    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        if (userId == Guid.Empty) return 0;

        return await _context.Follows
            .AsNoTracking()
            .CountAsync(f => f.Type == 1 && f.UserId == userId);
    }
    /// <summary>
    /// 追蹤統計（Followers=被追蹤數、Following=追蹤數）
    /// 供前端 Search 滑過展開使用
    /// </summary>
    public async Task<FollowStatsDto> GetFollowStatsAsync(Guid userId)
    {
        if (userId == Guid.Empty) return new FollowStatsDto(0, 0);

        // 直接重用上面兩個方法，邏輯一致
        var followers = await GetFollowerCountAsync(userId);
        var following = await GetFollowingCountAsync(userId);

        return new FollowStatsDto(followers, following);
    }

    public Task<List<Guid>> GetMutualFollowsAsync(Guid userId1, Guid userId2)
        {
            return Task.FromException<List<Guid>>(new NotImplementedException());
        }
    }
}