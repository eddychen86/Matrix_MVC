using Matrix.DTOs;

namespace Matrix.Services.Interfaces;

/// <summary>
/// 追蹤服務介面
/// </summary>
public interface IFollowService
{
    /// <summary>
    /// 追蹤使用者
    /// </summary>
    Task<bool> FollowUserAsync(Guid followerId, Guid followedId);

    /// <summary>
    /// 取消追蹤使用者
    /// </summary>
    Task<bool> UnfollowUserAsync(Guid followerId, Guid followedId);

    /// <summary>
    /// 檢查是否已追蹤
    /// </summary>
    Task<bool> IsFollowingAsync(Guid followerId, Guid followedId);

    /// <summary>
    /// 獲取追蹤者列表
    /// </summary>
    Task<(List<FollowDto> Followers, int TotalCount)> GetFollowersAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20);

    /// <summary>
    /// 獲取正在追蹤的使用者列表
    /// </summary>
    Task<(List<FollowDto> Following, int TotalCount)> GetFollowingAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20);

    /// <summary>
    /// 獲取追蹤者數量
    /// </summary>
    Task<int> GetFollowerCountAsync(Guid userId);

    /// <summary>
    /// 獲取正在追蹤的數量
    /// </summary>
    Task<int> GetFollowingCountAsync(Guid userId);

    /// <summary>
    /// 獲取兩個使用者的共同追蹤
    /// </summary>
    Task<List<Guid>> GetMutualFollowsAsync(Guid userId1, Guid userId2);
}