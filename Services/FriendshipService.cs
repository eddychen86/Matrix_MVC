using Matrix.Data;
using Matrix.DTOs;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 好友關係服務
    /// </summary>
    #pragma warning disable CS9113 // Parameter is unread
    public class FriendshipService(ApplicationDbContext _context) : IFriendshipService
    #pragma warning disable CS9113
    {

        public Task<bool> SendFriendRequestAsync(Guid senderId, Guid receiverId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> AcceptFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> RejectFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> RemoveFriendAsync(Guid userId1, Guid userId2)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
        {
            return Task.FromException<bool>(new NotImplementedException());
        }

        public Task<(List<FriendshipDto> Friends, int TotalCount)> GetFriendsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            return Task.FromException<(List<FriendshipDto>, int)>(new NotImplementedException());
        }

        public Task<(List<FriendshipDto> Requests, int TotalCount)> GetPendingRequestsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            return Task.FromException<(List<FriendshipDto>, int)>(new NotImplementedException());
        }

        public Task<int> GetFriendCountAsync(Guid userId)
        {
            return Task.FromException<int>(new NotImplementedException());
        }

        public Task<int> GetPendingRequestCountAsync(Guid userId)
        {
            return Task.FromException<int>(new NotImplementedException());
        }
    }
}