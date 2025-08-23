using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{
    /// <summary>
    /// 代表管理員活動記錄的實體 (原 LoginRecord 擴展為完整的活動追蹤)
    /// </summary>
    [Table("LoginRecords")] // 保持原表名以維持向後相容性
    public class AdminActivityLog
    {
        /// <summary>
        /// 活動記錄主鍵 ID (保持原有的 LoginId 名稱以維持向後相容性)
        /// </summary>
        [Key]
        public Guid LoginId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 管理員用戶的 UserId
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 管理員用戶名稱
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string AdminName { get; set; }

        /// <summary>
        /// 管理員角色等級 (1=管理員, 2=超級管理員)
        /// </summary>
        [Required]
        [Range(1, 2, ErrorMessage = "角色必須是 1 (管理員) 或 2 (超級管理員)")]
        public int Role { get; set; }

        /// <summary>
        /// 活動發生時的 IP 地址
        /// </summary>
        [Required]
        [StringLength(45)] // IPv6 最長 45 字元
        public required string IpAddress { get; set; }

        /// <summary>
        /// 瀏覽器和設備資訊
        /// </summary>
        [Required]
        [StringLength(500)]
        public required string UserAgent { get; set; }

        /// <summary>
        /// 登入時間 (Session 開始時間)
        /// </summary>
        public DateTime? LoginTime { get; set; }

        /// <summary>
        /// 登出時間 (Session 結束時間)
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// 活動發生時間
        /// </summary>
        [Required]
        public DateTime ActionTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 頁面路徑
        /// </summary>
        [StringLength(500)]
        public string PagePath { get; set; } = string.Empty;

        /// <summary>
        /// 操作類型 (LOGIN, LOGOUT, VIEW, CREATE, UPDATE, DELETE, ERROR)
        /// </summary>
        [Required]
        [StringLength(20)]
        public required string ActionType { get; set; }

        /// <summary>
        /// 活動描述
        /// </summary>
        [Required]
        [StringLength(1000)]
        public required string ActionDescription { get; set; }

        /// <summary>
        /// 在頁面停留的時間 (秒)
        /// </summary>
        public int Duration { get; set; } = 0;

        /// <summary>
        /// 操作是否成功
        /// </summary>
        [Required]
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// 錯誤訊息 (當 IsSuccessful = false 時)
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 關聯的用戶個人資料
        /// </summary>
        [ForeignKey("UserId")]
        public virtual required Person User { get; set; }
    }
}

