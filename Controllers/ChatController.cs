using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Matrix.Hubs;

namespace Matrix.Controllers
{
    public class ChatController : Controller
    {
        // SignalR 使用說明（MVC Controller）：
        // 1. 注入 IHubContext<MatrixHub> 來使用 SignalR 功能
        // 2. MatrixHub 會自動將已登入用戶加入到 "User_{PersonId}" 群組
        // 3. 在 Action 中可以透過 SignalR 推送即時更新到前端
        // 4. 前端 View 需要包含 SignalR JavaScript 連接和事件監聽

        private readonly IHubContext<MatrixHub> _hubContext;

        public ChatController(IHubContext<MatrixHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // 範例：聊天主頁面
        public ActionResult Index()
        {
            // 在 View 中需要建立 SignalR 連接：
            // const connection = new signalR.HubConnectionBuilder()
            //     .withUrl("/matrixHub")
            //     .build();
            // 
            // connection.start().then(function () {
            //     console.log("SignalR Connected");
            // });
            // 
            // // 監聽即時訊息
            // connection.on("ReceiveMessage", function (data) {
            //     // 處理接收到的訊息
            //     displayNewMessage(data.SenderId, data.Content, data.Timestamp);
            // });

            return View();
        }

        // 範例：在 Action 中使用 SignalR 推送通知
        // [HttpPost]
        // public async Task<IActionResult> SomeAction()
        // {
        //     var currentUserId = GetCurrentUserId(); // 取得當前用戶ID的方法
        //     
        //     // 推送給特定用戶
        //     await _hubContext.Clients.Group($"User_{targetUserId}")
        //         .SendAsync("SomeEvent", new { Message = "Hello from MVC!" });
        //     
        //     // 推送給所有線上用戶
        //     await _hubContext.Clients.Group("AllUsers")
        //         .SendAsync("GlobalNotification", new { Message = "系統公告" });
        //     
        //     return RedirectToAction("Index");
        // }

        // SignalR 在 MVC 中的常見使用場景：
        // 1. 頁面載入時自動標記訊息為已讀，並通知發送者
        // 2. 表單提交後推送即時更新給相關用戶
        // 3. 系統狀態變更時廣播給所有用戶
        // 4. 用戶進入/離開聊天室時的通知

        // 前端 JavaScript 事件監聽範例：
        // connection.on("ReceiveMessage", function(data) { /* 新訊息 */ });
        // connection.on("MessageRead", function(data) { /* 訊息已讀 */ });
        // connection.on("ConversationRead", function(data) { /* 對話已讀 */ });
        // connection.on("UserOnline", function(data) { /* 用戶上線 */ });
        // connection.on("UserOffline", function(data) { /* 用戶下線 */ });
    }
}
