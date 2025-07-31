using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 好友關係資料存取介面
    /// </summary>
    public interface IFriendshipRepository : IRepository<Friendship>
    {
        /// <summary>取得用戶的好友列表</summary>
        Task<IEnumerable<Friendship>> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得待處理的好友請求</summary>
        Task<IEnumerable<Friendship>> GetPendingFriendRequestsAsync(Guid userId);

        /// <summary>取得用戶發送的好友請求</summary>
        Task<IEnumerable<Friendship>> GetSentFriendRequestsAsync(Guid userId);

        /// <summary>檢查是否為好友關係</summary>
        Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);

        /// <summary>取得好友關係</summary>
        Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2);

        /// <summary>計算好友數量</summary>
        Task<int> CountFriendsAsync(Guid userId);

        /// <summary>計算待處理請求數量</summary>
        Task<int> CountPendingRequestsAsync(Guid userId);

        /// <summary>更新好友關係狀態</summary>
        Task UpdateFriendshipStatusAsync(Guid friendshipId, FriendshipStatus status);
    }
}