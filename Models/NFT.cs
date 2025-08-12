using System.ComponentModel.DataAnnotations;

namespace Matrix.Models
{
    /// <summary>
    /// NFT 數位收藏管理
    /// </summary>
    public class NFT
    {
        /// <summary>
        /// NFT ID
        /// </summary>
        [Key]
        public Guid NftId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 擁有者 ID（PersonId）
        /// </summary>
        [Required]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// NFT 名稱
        /// </summary>
        [Required]
        public string FileName { get; set; } = "";

        /// <summary>
        /// 檔案儲存路徑
        /// </summary>
        [Required]
        public string FilePath { get; set; } = "";

        /// <summary>
        /// NFT 購買時間（不是系統新增時間）
        /// </summary>
        public DateTime CollectTime { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Required]
        public string Currency { get; set; } = "";

        /// <summary>
        /// 價格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 與 Persons 關聯
        /// </summary>
        public virtual Person? Owner { get; set; }
    }
}