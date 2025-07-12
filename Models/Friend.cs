using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Friend
{
    /// <summary>
    /// 好友關係狀態的列舉
    /// </summary>
    public enum FriendshipStatus
    {
        Pending,    // 待確認
        Accepted,   // 已接受
        Declined,   // 已拒絕
        Blocked     // 已封鎖
    }

    /// <summary>
    /// 代表兩個使用者之間的好友關係
    /// </summary>
    public class Friends
    {
        // 建議使用一個獨立的主鍵，但也可以使用複合主鍵 (下面會說明)
        [Key]
        public int Id { get; set; }

        // 發出好友邀請的使用者 ID
        [Required]
        public required string UserId { get; set; }

        // 接收好友邀請的使用者 ID
        [Required]
        public required string FriendId { get; set; }

        // 好友關係的狀態 (待確認、已接受等)
        [Required]
        public FriendshipStatus Status { get; set; }

        // 建立關係的時間
        [Required]
        public DateTime RequestDate { get; set; }

        // --- 導覽屬性 (Navigation Properties) ---
        // 這些屬性讓 EF Core 知道如何建立關聯

        [ForeignKey("UserId")]
        public virtual required Person User { get; set; }

        [ForeignKey("FriendId")]
        public virtual required Person FriendUser { get; set; }
    }
}
