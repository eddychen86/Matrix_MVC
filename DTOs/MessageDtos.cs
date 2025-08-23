using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// 發送訊息的請求 DTO
    /// </summary>
    public class SendMessageDto
    {
        /// <summary>
        /// 接收者 ID
        /// </summary>
        [Required(ErrorMessage = "接收者 ID 不能為空")]
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// 訊息內容
        /// </summary>
        [Required(ErrorMessage = "訊息內容不能為空")]
        [StringLength(300, ErrorMessage = "訊息內容不能超過 300 個字元")]
        public required string Content { get; set; }
    }

    /// <summary>
    /// 訊息詳細資訊 DTO
    /// </summary>
    public class MessageDto
    {
        /// <summary>
        /// 訊息 ID
        /// </summary>
        public Guid MsgId { get; set; }

        /// <summary>
        /// 發送者 ID
        /// </summary>
        public Guid SentId { get; set; }

        /// <summary>
        /// 接收者 ID
        /// </summary>
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// 訊息內容
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// 發送時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否已讀
        /// </summary>
        public int IsRead { get; set; }

        /// <summary>
        /// 發送者姓名
        /// </summary>
        public string? SenderName { get; set; }

        /// <summary>
        /// 發送者頭像
        /// </summary>
        public string? SenderAvatar { get; set; }
    }

    /// <summary>
    /// 對話列表項目 DTO
    /// </summary>
    public class ConversationItemDto
    {
        /// <summary>
        /// 對話伙伴 ID
        /// </summary>
        public Guid PartnerId { get; set; }

        /// <summary>
        /// 對話伙伴姓名
        /// </summary>
        public required string PartnerName { get; set; }

        /// <summary>
        /// 對話伙伴頭像
        /// </summary>
        public string? PartnerAvatar { get; set; }

        /// <summary>
        /// 最後一則訊息內容
        /// </summary>
        public required string LastMessage { get; set; }

        /// <summary>
        /// 最後訊息時間
        /// </summary>
        public DateTime LastMessageTime { get; set; }

        /// <summary>
        /// 未讀訊息數量
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// 是否為自己發送的最後一則訊息
        /// </summary>
        public bool IsLastMessageFromMe { get; set; }
    }

    /// <summary>
    /// 訊息搜尋請求 DTO
    /// </summary>
    public class SearchMessageDto
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        [Required(ErrorMessage = "搜尋關鍵字不能為空")]
        [StringLength(100, ErrorMessage = "搜尋關鍵字不能超過 100 個字元")]
        public required string Keyword { get; set; }

        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁數量
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 獲取對話記錄請求 DTO
    /// </summary>
    public class GetConversationDto
    {
        /// <summary>
        /// 對話伙伴 ID
        /// </summary>
        [Required(ErrorMessage = "對話伙伴 ID 不能為空")]
        public Guid PartnerId { get; set; }

        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁數量
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 標記訊息已讀請求 DTO
    /// </summary>
    public class MarkMessageReadDto
    {
        /// <summary>
        /// 訊息 ID
        /// </summary>
        [Required(ErrorMessage = "訊息 ID 不能為空")]
        public Guid MessageId { get; set; }
    }

    /// <summary>
    /// 標記對話已讀請求 DTO
    /// </summary>
    public class MarkConversationReadDto
    {
        /// <summary>
        /// 對話伙伴 ID
        /// </summary>
        [Required(ErrorMessage = "對話伙伴 ID 不能為空")]
        public Guid PartnerId { get; set; }
    }

    /// <summary>
    /// 刪除訊息請求 DTO
    /// </summary>
    public class DeleteMessageDto
    {
        /// <summary>
        /// 訊息 ID
        /// </summary>
        [Required(ErrorMessage = "訊息 ID 不能為空")]
        public Guid MessageId { get; set; }
    }

    /// <summary>
    /// 刪除對話請求 DTO
    /// </summary>
    public class DeleteConversationDto
    {
        /// <summary>
        /// 對話伙伴 ID
        /// </summary>
        [Required(ErrorMessage = "對話伙伴 ID 不能為空")]
        public Guid PartnerId { get; set; }
    }

    /// <summary>
    /// API 回應基礎 DTO
    /// </summary>
    public class MessageResponseDto<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 回應訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 回應資料
        /// </summary>
        public T? Data { get; set; }
    }

    /// <summary>
    /// 分頁回應 DTO
    /// </summary>
    public class PagedResponseDto<T>
    {
        /// <summary>
        /// 資料列表
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁數量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;
    }
}