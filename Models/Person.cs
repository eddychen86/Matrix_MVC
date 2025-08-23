using System.ComponentModel.DataAnnotations;

namespace Matrix.Models
{

    /// <summary>
    /// 代表用戶的個人資料實體
    /// </summary>
    public class Person
    {
        /// <summary>
        /// 個人資料的 ID
        /// </summary>
        [Key]
        public Guid PersonId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 關聯用戶的 UserId，外鍵連接到 User
        /// </summary>
        public Guid UserId { get; set; }

        public virtual User? User { get; set; }

        /// <summary>
        /// 用戶的顯示名稱，最大長度為50個字元
        /// </summary>
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 用戶的個人簡介，最大長度為300個字元
        /// </summary>
        [MaxLength(300)]
        public string? Bio { get; set; }

        /// <summary>
        /// 用戶頭像的檔案路徑
        /// </summary>
        [MaxLength(2048)]
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 用戶個人頁面橫幅的檔案路徑
        /// </summary>
        [MaxLength(2048)]
        public string? BannerPath { get; set; }

        /// <summary>
        /// 用戶的外部網站連結 1
        /// </summary>
        [MaxLength(2048)]
        public string? Website1 { get; set; }

        /// <summary>
        /// 用戶的外部網站連結 2
        /// </summary>
        [MaxLength(2048)]
        public string? Website2 { get; set; }

        /// <summary>
        /// 用戶的外部網站連結 3
        /// </summary>
        [MaxLength(2048)]
        public string? Website3 { get; set; }

        /// <summary>
        /// 用戶的隱私設定，0表示公開，1表示私人
        /// </summary>
        public int IsPrivate { get; set; } = 0;

        /// <summary>
        /// 用戶的區塊鏈錢包地址
        /// </summary>
        public string? WalletAddress { get; set; }

        /// <summary>
        /// 個人資料的最後修改時間
        /// </summary>
        public DateTime? ModifyTime { get; set; }

        // Navigation properties
        /// <summary>
        /// 關聯的用戶帳號，一對一關聯
        /// </summary>

        /// <summary>
        /// 用戶發布的文章集合
        /// </summary>
        public ICollection<Article> Articles { get; set; } = [];

        /// <summary>
        /// 用戶發布的回覆集合
        /// </summary>
        public ICollection<Reply> Replies { get; set; } = [];

        /// <summary>
        /// 用戶的讚與收藏記錄集合
        /// </summary>
        public ICollection<PraiseCollect> PraiseCollects { get; set; } = [];

        /// <summary>
        /// 用戶的關注記錄集合
        /// </summary>
        public ICollection<Follow> Follows { get; set; } = [];

        /// <summary>
        /// 用戶發送的通知集合
        /// </summary>
        public ICollection<Notification> NotificationsSent { get; set; } = [];

        /// <summary>
        /// 用戶接收的通知集合
        /// </summary>
        public ICollection<Notification> NotificationsReceived { get; set; } = [];

        /// <summary>
        /// 用戶提交的舉報集合
        /// </summary>
        public ICollection<Report> ReportsMade { get; set; } = [];

        /// <summary>
        /// 用戶處理的舉報集合
        /// </summary>
        public ICollection<Report> ReportsResolved { get; set; } = [];

        /// <summary>
        /// 用戶的管理員活動記錄集合（原登入記錄擴展）
        /// </summary>
        public ICollection<AdminActivityLog> LoginRecords { get; set; } = [];

        /// <summary>
        /// 用戶發起的好友關係集合（我加別人）
        /// </summary>
        public ICollection<Friendship> Friends { get; set; } = [];

        /// <summary>
        /// 用戶接收的好友關係集合（別人加我）
        /// </summary>
        public ICollection<Friendship> FriendOf { get; set; } = [];

        /// <summary>
        /// 用戶擁有的 NFT 集合
        /// </summary>
        public ICollection<NFT> NFTs { get; set; } = [];
    }

}
