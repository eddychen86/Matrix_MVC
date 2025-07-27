using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{

    /// <summary>
    /// 代表系統用戶帳號的實體
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用戶的唯一識別碼
        /// 改用 UUID 以確保唯一性和安全性，並以 ArrayExtension.GenerateOrdered(1)[0] 方法生成一個劇時間排序的唯一的值
        /// </summary>
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 用戶的權限等級，0表示一般用戶，其他值表示不同權限等級
        /// </summary>
        public required int Role { get; set; } = 0;

        /// <summary>
        /// 用戶的顯示名稱，用於登入和顯示
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// 用戶的電子郵件地址，用於登入和通知
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        /// <summary>
        /// 用戶的密碼（加密儲存）
        /// </summary>
        [Required]
        public string Password { get; set; } = null!;

        /// <summary>
        /// 確認密碼欄位，僅用於表單驗證，不儲存到資料庫
        /// </summary>
        [NotMapped]
        public string? PasswordConfirm { get; set; }

        /// <summary>
        /// 用戶所在的國家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 用戶的性別，可能的值取決於系統設定
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// 帳號的建立時間
        /// </summary>
        public required DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 用戶最後一次登入的時間
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 帳號狀態，0表示啟用，1表示停用，2表示被封禁
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 關聯的用戶個人資料
        /// </summary>
        public virtual Person? Person { get; set; }
    }
}