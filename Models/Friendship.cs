using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

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
public class Friendship
{
    /// <summary>
    /// 好友關係的唯一識別碼
    /// </summary>
    [Key]
    public Guid FriendshipId { get; set; }

    /// <summary>
    /// 發出好友邀請的使用者識別碼
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 接收好友邀請的使用者識別碼
    /// </summary>
    [Required]
    public Guid FriendId { get; set; }

    /// <summary>
    /// 好友關係的狀態，包括待確認、已接受、已拒絕或已封鎖
    /// </summary>
    [Required]
    public FriendshipStatus Status { get; set; }

    /// <summary>
    /// 好友邀請發送的時間
    /// </summary>
    [Required]
    public DateTime RequestDate { get; set; }

    // --- 導覽屬性 (Navigation Properties) ---
    // 這些屬性讓 EF Core 知道如何建立關聯

    /// <summary>
    /// 發出好友邀請的使用者
    /// </summary>
    [ForeignKey("UserId")]
    public virtual required Person Requester { get; set; }

    /// <summary>
    /// 接收好友邀請的使用者
    /// </summary>
    [ForeignKey("FriendId")]
    public virtual required Person Recipient { get; set; }
}