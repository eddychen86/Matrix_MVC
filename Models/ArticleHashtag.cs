using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{

    /// <summary>
    /// 代表文章與標籤之間的多對多關聯
    /// </summary>
    public class ArticleHashtag
    {
        /// <summary>
        /// 關聯文章的 ID，作為複合主鍵的一部分
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 關聯標籤的唯一識別碼，作為複合主鍵的一部分
        /// </summary>
        public Guid TagId { get; set; }

        /// <summary>
        /// 關聯的文章實體
        /// </summary>
        [ForeignKey("ArticleId")]
        public virtual Article? Article { get; set; }

        /// <summary>
        /// 關聯的標籤實體
        /// </summary>
        [ForeignKey("TagId")]
        public virtual Hashtag? Hashtag { get; set; }
    }
}

