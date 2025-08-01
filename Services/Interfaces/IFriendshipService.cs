namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 好友關係服務介面
    /// </summary>
    public interface IFriendshipService
    {
        /// <summary>
        /// 發送好友邀請
        /// </summary>
        Task<bool> SendFriendRequestAsync(Guid senderId, Guid receiverId);

        /// <summary>
        /// 接受好友邀請
        /// </summary>
        Task<bool> AcceptFriendRequestAsync(Guid friendshipId, Guid userId);

        /// <summary>
        /// 拒絕好友邀請
        /// </summary>
        /// <param name="friendshipId">好友關係 ID</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> RejectFriendRequestAsync(Guid friendshipId, Guid userId);

        /// <summary>
        /// 移除好友關係
        /// </summary>
        /// <param name="userId1">使用者1 ID</param>
        /// <param name="userId2">使用者2 ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> RemoveFriendAsync(Guid userId1, Guid userId2);

        /// <summary>
        /// 檢查是否為好友關係
        /// </summary>
        /// <param name="userId1">使用者1 ID</param>
        /// <param name="userId2">使用者2 ID</param>
        /// <returns>是否為好友</returns>
        Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);

        /// <summary>
        /// 獲取好友列表
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>好友列表和總數量</returns>
        Task<(List<FriendshipDto> Friends, int TotalCount)> GetFriendsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// 獲取待處理的好友邀請列表
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>待處理邀請列表和總數量</returns>
        Task<(List<FriendshipDto> Requests, int TotalCount)> GetPendingRequestsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// 獲取好友數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>好友數量</returns>
        Task<int> GetFriendCountAsync(Guid userId);

        /// <summary>
        /// 獲取待處理好友邀請數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>待處理邀請數量</returns>
        Task<int> GetPendingRequestCountAsync(Guid userId);
    }
}