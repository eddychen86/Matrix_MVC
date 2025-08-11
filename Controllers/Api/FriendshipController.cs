using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using Matrix.ViewModels;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [Route("api/friends")]
    [ApiController]
    public class FriendshipController(
        IUserService userService,
        IFriendshipRepository friendshipRepository,
        ILogger<FriendshipController> logger
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly ILogger<FriendshipController> _logger = logger;

        /// <summary>
        /// 依 username 取得好友列表
        /// 兼容舊路徑：GET /api/Friendship/{username}/friends
        /// 推薦新路徑：GET /api/friends/{username}?status=accepted|pending|declined|blocked|all
        /// </summary>
        public async Task<IActionResult> GetFriendsByUsername(
            [FromRoute] string username,
            [FromQuery] string? status = "accepted")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(new { message = "username 不可為空" });
                }

                // 解析 username 對應的使用者
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "找不到指定使用者" });
                }
                // 注意：Friendship 的 UserId/FriendId 與 Person.PersonId 對應
                var person = await _userService.GetProfileByIdAsync(user.UserId);
                if (person == null)
                {
                    return NotFound(new { message = "找不到指定使用者的個人資料" });
                }

                // 解析狀態
                FriendshipStatus? statusFilter = ParseStatus(status);
                // 預設為 accepted（若不提供或提供空字串）
                if (statusFilter == null && !IsAllStatus(status))
                {
                    statusFilter = FriendshipStatus.Accepted;
                }

                // 取出好友關係
                var friendships = await _friendshipRepository.GetFriendsByStatusAsync(
                    person.PersonId,
                    IsAllStatus(status) ? null : statusFilter
                );
                var total = friendships.Count();

                // 對每筆關係，找出「對方」的 Person 當作好友資料
                var items = friendships.Select(f =>
                {
                    var friend = f.UserId == person.PersonId ? f.Recipient : f.Requester;
                    return new FriendListViewModel
                    {
                        UserId = friend.UserId.ToString(),
                        UserName = friend.DisplayName ?? "未知用戶",
                        AvatarPath = string.IsNullOrEmpty(friend.AvatarPath) ? null : friend.AvatarPath
                    };
                }).ToList();

                return Ok(new { items, totalCount = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得好友列表時發生例外，username: {Username}", username);
                return StatusCode(500, new { message = "伺服器內部錯誤" });
            }
        }

        /// <summary>
        /// 依 username 取得好友列表（新路徑）
        /// GET /api/friends/{username}?status=accepted|pending|declined|blocked|all
        /// </summary>
        [HttpGet("{username}")]
        public Task<IActionResult> GetFriendsByUsernameNew(
            [FromRoute] string username,
            [FromQuery] string? status = "accepted")
        {
            return GetFriendsByUsername(username, status);
        }

        /// <summary>
        /// 取得目前登入者的好友列表
        /// GET /api/friends?status=accepted|pending|declined|blocked|all
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> GetMyFriends(
            [FromQuery] string? status = "accepted")
        {
            try
            {
                // 從認證中取得使用者名稱
                var currentUserName = HttpContext.Items["UserName"] as string;
                if (string.IsNullOrWhiteSpace(currentUserName))
                {
                    return Unauthorized(new { message = "用戶未認證且未提供 username" });
                }

                return await GetFriendsByUsername(currentUserName, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得目前用戶好友列表時發生例外");
                return StatusCode(500, new { message = "伺服器內部錯誤" });
            }
        }

        private static bool IsAllStatus(string? status)
        {
            return !string.IsNullOrWhiteSpace(status) && status.Equals("all", StringComparison.OrdinalIgnoreCase);
        }

        private static FriendshipStatus? ParseStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return null;
            return status.ToLowerInvariant() switch
            {
                "pending" => FriendshipStatus.Pending,
                "accepted" => FriendshipStatus.Accepted,
                "declined" => FriendshipStatus.Declined,
                "blocked" => FriendshipStatus.Blocked,
                "all" => null,
                _ => null
            };
        }
    }
}
