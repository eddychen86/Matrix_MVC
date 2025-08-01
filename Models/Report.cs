using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{

    /// <summary>
    /// 代表用戶舉報的實體
    /// </summary>
    public class Report
    {
        /// <summary>
        /// 舉報的唯一識別碼
        /// 改用 UUID 以確保唯一性和安全性，並以 ArrayExtension.GenerateOrdered(1)[0] 方法生成一個劇時間排序的唯一的值
        /// </summary>
        [Key]
        public Guid ReportId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 提交舉報的 UserId
        /// </summary>
        public Guid ReporterId { get; set; }

        /// <summary>
        /// 被舉報的 ArticleId / UserId
        /// </summary>
        public Guid TargetId { get; set; }

        /// <summary>
        /// 舉報的類型，用於區分不同種類的舉報
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 舉報的原因說明，最大長度為500個字元
        /// </summary>
        [Required, MaxLength(500)]
        public required string Reason { get; set; }

        /// <summary>
        /// 舉報的處理狀態，0表示待處理，其他值表示不同處理狀態
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 處理舉報的 Admin UserId
        /// </summary>
        public Guid? ResolverId { get; set; }

        /// <summary>
        /// 舉報的處理時間
        /// </summary>
        public DateTime? ProcessTime { get; set; }

        /// <summary>
        /// 關聯的舉報者個人資料
        /// </summary>
        [ForeignKey("ReporterId")]
        public virtual Person? Reporter { get; set; }

        /// <summary>
        /// 關聯的處理者個人資料
        /// </summary>
        [ForeignKey("ResolverId")]
        public virtual Person? Resolver { get; set; }
        // TargetId 需用商業邏輯處理，不設外鍵
    }
}

