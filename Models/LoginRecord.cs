using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models{

    /// <summary>
    /// 代表用戶登入記錄的實體
    /// </summary>
    public class LoginRecord
    {
        /// <summary>
        /// 登入記錄的唯一識別碼
        /// </summary>
        [Key]
        public Guid LoginId { get; set; }
        
        /// <summary>
        /// 登入用戶的識別碼
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// 用戶登入時的 IP 地址
        /// </summary>
        public required string IpAddress { get; set; }
        
        /// <summary>
        /// 用戶登入時使用的瀏覽器和設備資訊
        /// </summary>
        public required string UserAgent { get; set; }
        
        /// <summary>
        /// 登入的時間
        /// </summary>
        public DateTime LoginTime { get; set; }
        
        /// <summary>
        /// 用戶的操作歷史記錄
        /// </summary>
        public required string History { get; set; }

        /// <summary>
        /// 關聯的用戶個人資料
        /// </summary>
        [ForeignKey("UserId")]
        public virtual required Person User { get; set; }
    }
}

