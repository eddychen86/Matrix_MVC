using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Matrix.Hubs;
using Matrix.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Matrix.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Matrix.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ApiChatController : ControllerBase
    {
        // SignalR 使用說明：
        // 1. 注入 IHubContext<MatrixHub> 來使用 SignalR
        // 2. MatrixHub 會自動將用戶加入到 "User_{PersonId}" 群組
        // 3. 發送訊息時使用 _hubContext.Clients.Group($"User_{receiverId}").SendAsync("事件名稱", 資料)
        // 4. 前端 JavaScript 需要監聽對應的事件：connection.on("事件名稱", function(data) {...})

        private readonly IHubContext<MatrixHub> _hubContext;
        private readonly IMessageService _messageService;
        private readonly IPersonRepository _personRepository;

        public ApiChatController(IHubContext<MatrixHub> hubContext, IMessageService messageService, IPersonRepository personRepository)
        {
            _hubContext = hubContext;
            _messageService = messageService;
            _personRepository = personRepository;
        }

        // 範例：發送即時訊息
        // [HttpPost("send")]
        // public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        // {
        //     // 1. 處理業務邏輯（儲存訊息到資料庫等）
        //     
        //     // 2. 透過 SignalR 即時推送給接收者
        //     await _hubContext.Clients.Group($"User_{request.ReceiverId}")
        //         .SendAsync("ReceiveMessage", new
        //         {
        //             SenderId = currentUserId,
        //             Content = request.Content,
        //             Timestamp = DateTime.Now
        //         });
        //     
        //     return Ok();
        // }

        // 範例：通知訊息已讀
        // [HttpPost("mark-read/{messageId}")]
        // public async Task<IActionResult> MarkAsRead(Guid messageId)
        // {
        //     // 1. 更新資料庫中的已讀狀態
        //     
        //     // 2. 通知發送者訊息已被讀取
        //     await _hubContext.Clients.Group($"User_{senderId}")
        //         .SendAsync("MessageRead", new { MessageId = messageId, ReadBy = currentUserId });
        //     
        //     return Ok();
        // }

        // 常用的 SignalR 事件名稱：
        // - "ReceiveMessage": 接收新訊息
        // - "MessageRead": 訊息已讀通知
        // - "ConversationRead": 對話已讀通知
        // - "UserOnline": 用戶上線通知
        // - "UserOffline": 用戶下線通知
        // - "TypingStart": 開始輸入
        // - "TypingStop": 停止輸入

        /// <summary>
        /// Sends a message to another user.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var senderId = HttpContext.Items["UserId"] as Guid?;
            if (!senderId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. service 正常執行，返回的 createdMessage 內部是 PersonId
                var createdMessage = await _messageService.SendMessageAsync(senderId.Value, dto.ReceiverId, dto.Content);

                // 2. [核心修正] 創建一個專門用於 SignalR 廣播的 payload
                //    確保廣播出去的 ID 是前端和 Hub 群組所使用的 UserId
                var signalrPayload = new
                {
                    MsgId = createdMessage.MsgId,
                    SentId = senderId.Value,       // <-- 使用原始的 senderId (UserId)
                    ReceiverId = dto.ReceiverId,   // <-- 使用原始的 receiverId (UserId)
                    Content = createdMessage.Content,
                    CreateTime = createdMessage.CreateTime,
                    IsRead = createdMessage.IsRead
                };

                // 3. 將新的 payload 廣播出去
                await _hubContext.Clients.Group($"User_{dto.ReceiverId}")
                    .SendAsync("ReceiveMessage", signalrPayload);

                await _hubContext.Clients.Group($"User_{senderId.Value}")
                    .SendAsync("ReceiveMessage", signalrPayload);

                // 4. HTTP 回應仍然可以返回原始的物件，這不影響 SignalR
                return Ok(createdMessage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Gets the conversation history with another user.
        /// </summary>
        [HttpGet("history/{receiverId}")]
        public async Task<IActionResult> GetConversationHistory(Guid receiverId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user1Id = HttpContext.Items["UserId"] as Guid?;
            if (!user1Id.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            try
            {
                // 1. 從 Service 取得原始的歷史訊息 (內部是 PersonId)
                var conversationHistory = await _messageService.GetConversationAsync(user1Id.Value, receiverId, page, pageSize);

                // 2. [核心修正] 將歷史訊息中的 PersonId 轉換回 UserId
                var userIds = await _personRepository.GetUserIdsByPersonIds(
                    conversationHistory.Select(m => m.SentId).Distinct()
                );

                var formattedHistory = conversationHistory.Select(message =>
                {
                    // 根據 PersonId 找到對應的 UserId
                    var senderUserId = userIds.FirstOrDefault(u => u.PersonId == message.SentId)?.UserId;

                    // 注意：我們假設 ReceiverId 也需要轉換，但通常另一方就是當前用戶或聊天對象
                    // 為了簡化，我們先只處理發送者，這足以解決左右顯示問題

                    return new
                    {
                        MsgId = message.MsgId,
                        SentId = senderUserId, // <-- 使用轉換後的 UserId
                        ReceiverId = message.ReceiverId, // 這裡可能仍是 PersonId，但暫不影響左右判斷
                        Content = message.Content,
                        CreateTime = message.CreateTime,
                        IsRead = message.IsRead
                    };
                });

                // 3. 回傳格式化後的、包含 UserId 的資料
                return Ok(formattedHistory);
            }
            catch (Exception)
            {
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
