using System.ComponentModel.DataAnnotations;

namespace Matrix.Models
{

    /// <summary>
    /// 代表系統中的文章實體
    /// </summary>
    public class Article
    {
        /// <summary>
        /// 文章的 ID
        /// 改用 UUID 以確保唯一性和安全性，並以 ArrayExtension.GenerateOrdered(1)[0] 方法生成一個劇時間排序的唯一的值
        /// </summary>
        [Key]
        public Guid ArticleId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 文章作者的 UserId
        /// </summary>
        [Required]
        public Guid AuthorId { get; set; }

        /// <summary>
        /// 文章的內容文字
        /// </summary>
        [Required, MaxLength(4000)]
        public required string Content { get; set; }

        /// <summary>
        /// 文章的公開狀態，0表示公開，1表示私人
        /// </summary>
        public int IsPublic { get; set; } = 0;

        /// <summary>
        /// 文章的狀態，0表示正常，其他值表示不同狀態（如刪除、審核中等）
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 文章的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 文章獲得的讚數量
        /// </summary>
        public int PraiseCount { get; set; } = 0;

        /// <summary>
        /// 文章被收藏的次數
        /// </summary>
        public int CollectCount { get; set; } = 0;

        // Navigation properties
        /// <summary>
        /// 文章作者的個人資料連結
        /// </summary>
        public virtual Person? Author { get; set; }

        /// <summary>
        /// 文章關聯的標籤集合
        /// </summary>
        public virtual ICollection<ArticleHashtag>? ArticleHashtags { get; set; }

        /// <summary>
        /// 文章的回覆集合
        /// </summary>
        public virtual ICollection<Reply>? Replies { get; set; }

        /// <summary>
        /// 文章的讚與收藏記錄集合
        /// </summary>
        public virtual ICollection<PraiseCollect>? PraiseCollects { get; set; }

        /// <summary>
        /// 文章的附件集合
        /// </summary>
        public virtual ICollection<ArticleAttachment>? Attachments { get; set; }
    }
}