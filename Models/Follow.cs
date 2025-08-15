using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models
{

    /// <summary>
    /// 代表用戶關注關係的實體
    /// </summary>
    public class Follow
    {
        /// <summary>
        /// 被關注對象的「實體主鍵」
        /// Type=1(使用者) → 對應 Persons.PersonId
        /// Type=0(文章)   → 對應 Articles.ArticleId
        /// </summary>
        [Key]
        public Guid FollowId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 關注者的 UserId
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 被關注對象的 UserId
        /// </summary>
        public Guid FollowedId { get; set; }

        /// <summary>
        /// 關注類型，用於區分不同種類的關注關係
        /// 0: 文章、1: 使用者
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 關注的時間
        /// </summary>
        public DateTime FollowTime { get; set; }

        /// <summary>
        /// 關注者的個人資料連結
        /// </summary>
        [ForeignKey("UserId")]
        public virtual required Person User { get; set; }

        // FollowedId 需用商業邏輯處理，不設外鍵，可能指向不同類型的實體
    }
}

