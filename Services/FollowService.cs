using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Services.Interfaces;

namespace Matrix.Services;

/// <summary>
/// 追蹤服務
/// </summary>
public class FollowService : IFollowService
{
    private readonly ApplicationDbContext _context;

    public FollowService(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public Task<int> GetFollowerCountAsync(Guid userId)
    {
        return Task.FromException<int>(new NotImplementedException());
    }

    public Task<int> GetFollowingCountAsync(Guid userId)
    {
        return Task.FromException<int>(new NotImplementedException());
    }

    public Task<List<Guid>> GetMutualFollowsAsync(Guid userId1, Guid userId2)
    {
        return Task.FromException<List<Guid>>(new NotImplementedException());
    }
}