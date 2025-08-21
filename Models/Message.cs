using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{
    /// <summary>
    /// 代表聊天的實體
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 回覆的唯一識別碼
        /// </summary>
        [Key]
        public Guid MsgId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 聊天的 UserId
        /// </summary>
        [Required]
        public Guid SentId { get; set; }

        /// <summary>
        /// 接收聊天的 UserId
        /// </summary>
        [Required]
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        [Required, MaxLength(300)]
        public required string Content { get; set; }

        /// <summary>
        /// 發送時間
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已讀 (0: 未讀, 1: 已讀)
        /// </summary>
        public int IsRead { get; set; } = 0;

        /// <summary>
        /// 關聯的用戶個人資料
        /// </summary>
        [ForeignKey("SentId")]
        public virtual Person? User { get; set; }
    }
}