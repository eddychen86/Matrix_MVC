using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models{

    /// <summary>
    /// 代表系統通知的實體
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 通知的唯一識別碼
        /// </summary>
        [Key]
        public Guid NotifyId { get; set; }
        
        /// <summary>
        /// 接收通知的用戶識別碼
        /// </summary>
        public Guid GetId { get; set; }
        
        /// <summary>
        /// 發送通知的用戶識別碼
        /// </summary>
        public Guid SendId { get; set; }
        
        /// <summary>
        /// 通知的類型，用於區分不同種類的通知
        /// </summary>
        public int Type { get; set; }
        
        /// <summary>
        /// 通知的閱讀狀態，0表示未讀，1表示已讀
        /// </summary>
        public int IsRead { get; set; } = 0;
        
        /// <summary>
        /// 通知發送的時間
        /// </summary>
        public DateTime SentTime { get; set; }
        
        /// <summary>
        /// 通知被閱讀的時間
        /// </summary>
        public DateTime? IsReadTime { get; set; }

        /// <summary>
        /// 通知接收者的個人資料連結
        /// </summary>
        [ForeignKey("GetId")]
        public virtual required Person Receiver { get; set; }
        
        /// <summary>
        /// 通知發送者的個人資料連結
        /// </summary>
        [ForeignKey("SendId")]
        public virtual required Person Sender { get; set; }
    }
}

