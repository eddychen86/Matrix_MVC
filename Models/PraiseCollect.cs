using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{

    /// <summary>
    /// 代表用戶對文章的讚或收藏操作
    /// </summary>
    public class PraiseCollect
    {
        /// <summary>
        /// 讚或收藏事件的 ID
        /// 改用 UUID 以確保唯一性和安全性，並以 ArrayExtension.GenerateOrdered(1)[0] 方法生成一個劇時間排序的唯一的值
        /// </summary>
        [Key]
        public Guid EventId { get; set; } = ArrayExtension.GenerateOrdered(1)[0];

        /// <summary>
        /// 操作類型，例如：0表示讚，1表示收藏
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 執行操作的 UserId
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 被讚或收藏的 ArticleId
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 操作的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

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

