using System.ComponentModel.DataAnnotations;

namespace Matrix.Models
{

    /// <summary>
    /// 代表文章的附件檔案
    /// </summary>
    public class ArticleAttachment
    {
        /// <summary>
        /// 附件檔案的 ID
        /// </summary>
        [Key]
        public Guid FileId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 關聯文章的 ArticleId
        /// </summary>
        [Required]
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 附件檔案的儲存路徑
        /// </summary>
        [Required]
        public string FilePath { get; set; } = "";

        /// <summary>
        /// 附件的類型，例如 "image" 或 "file"
        /// </summary>
        [Required]
        public string Type { get; set; } = ""; // "image" 或 "file"

        /// <summary>
        /// 附件的原始檔案名稱
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 附件的MIME類型，用於確定檔案的格式和類型
        /// </summary>
        public string? MimeType { get; set; }

        // 導覽屬性
        /// <summary>
        /// 此附件所屬的文章
        /// </summary>
        public virtual Article? Article { get; set; }
    }
}

