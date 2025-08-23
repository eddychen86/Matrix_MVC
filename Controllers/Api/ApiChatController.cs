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

        public ApiChatController(IHubContext<MatrixHub> hubContext, IMessageService messageService)
        {
            _hubContext = hubContext;
            _messageService = messageService;
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
                var createdMessage = await _messageService.SendMessageAsync(senderId.Value, dto.ReceiverId, dto.Content);

                await _hubContext.Clients.Group($"User_{dto.ReceiverId}")
                    .SendAsync("ReceiveMessage", createdMessage);
                
                await _hubContext.Clients.Group($"User_{senderId.Value}")
                    .SendAsync("ReceiveMessage", createdMessage);

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
            catch (Exception ex)
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
                var conversation = await _messageService.GetConversationAsync(user1Id.Value, receiverId, page, pageSize);
                return Ok(conversation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
