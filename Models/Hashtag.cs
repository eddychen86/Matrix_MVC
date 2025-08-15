using System.ComponentModel.DataAnnotations;

namespace Matrix.Models
{

    /// <summary>
    /// 代表系統中的標籤實體
    /// </summary>
    public class Hashtag
    {
        /// <summary>
        /// 標籤的唯一識別碼
        /// </summary>
        [Key]
        public Guid TagId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 標籤的文字內容，最大長度為10個字元
        /// </summary>
        [Required, MaxLength(10)]
        public required string Content { get; set; }

        /// <summary>
        /// 標籤的狀態，0表示正常，其他值表示不同狀態
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 與此標籤關聯的文章集合
        /// </summary>
        public virtual required ICollection<ArticleHashtag> ArticleHashtags { get; set; }
    }
}

