using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models{

    /// <summary>
    /// 代表文章回覆的實體
    /// </summary>
    public class Reply
    {
        /// <summary>
        /// 回覆的唯一識別碼
        /// 改用 UUID 以確保唯一性和安全性，並以 ArrayExtension.GenerateOrdered(1)[0] 方法生成一個劇時間排序的唯一的值
        /// </summary>
        [Key]
        public UUID ReplyId { get; set; } = ArrayExtension.GenerateOrdered(1)[0];
        
        /// <summary>
        /// 發表回覆的 UserId
        /// </summary>
        [Required]
        public UUID UserId { get; set; }
        
        /// <summary>
        /// 被回覆的 ArticleId
        /// </summary>
        [Required]
        public UUID ArticleId { get; set; }
        
        /// <summary>
        /// 回覆的內容文字，最大長度為1000個字元
        /// </summary>
        [Required, MaxLength(1000)]
        public required string Content { get; set; }
        
        /// <summary>
        /// 回覆的發表時間
        /// </summary>
        public DateTime ReplyTime { get; set; }

        /// <summary>
        /// 關聯的用戶個人資料
        /// </summary>
        [ForeignKey("UserId")]
        public virtual Person? User { get; set; }
        
        /// <summary>
        /// 關聯的文章
        /// </summary>
        [ForeignKey("ArticleId")]
        public virtual Article? Article { get; set; }
    }
}

