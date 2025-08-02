using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 追蹤關係資料存取介面
    /// </summary>
    public interface IFollowRepository : IRepository<Follow>
    {
        /// <summary>取得用戶的追蹤列表</summary>
        Task<IEnumerable<Follow>> GetFollowingAsync(Guid followerId, int page = 1, int pageSize = 20);

        /// <summary>取得用戶的粉絲列表</summary>
        Task<IEnumerable<Follow>> GetFollowersAsync(Guid followedId, int page = 1, int pageSize = 20);

        /// <summary>檢查是否已追蹤</summary>
        Task<bool> IsFollowingAsync(Guid followerId, Guid followedId);

        /// <summary>取得追蹤關係</summary>
        Task<Follow?> GetFollowRelationAsync(Guid followerId, Guid followedId);

        /// <summary>計算追蹤數量</summary>
        Task<int> CountFollowingAsync(Guid followerId);

        /// <summary>計算粉絲數量</summary>
        Task<int> CountFollowersAsync(Guid followedId);

        /// <summary>取得相互追蹤的用戶</summary>
        Task<IEnumerable<Follow>> GetMutualFollowsAsync(Guid userId);
    }
}