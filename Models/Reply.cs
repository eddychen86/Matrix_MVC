using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

/// <summary>
/// 代表文章回覆的實體
/// </summary>
public class Reply
{
    /// <summary>
    /// 回覆的唯一識別碼
    /// </summary>
    [Key]
    public Guid ReplyId { get; set; }
    
    /// <summary>
    /// 發表回覆的用戶識別碼
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 被回覆的文章識別碼
    /// </summary>
    [Required]
    public Guid ArticleId { get; set; }
    
    /// <summary>
    /// 回覆的內容文字，最大長度為1000個字元
    /// </summary>
    [Required, MaxLength(1000)]
    public required string Content { get; set; }
    
    /// <summary>
    /// 回覆的發表時間
    /// </summary>
    public DateTime ReplyTime { get; set; }

    /// <summary>
    /// 關聯的用戶個人資料
    /// </summary>
    [ForeignKey("UserId")]
    public virtual Person? User { get; set; }
    
    /// <summary>
    /// 關聯的文章
    /// </summary>
    [ForeignKey("ArticleId")]
    public virtual Article? Article { get; set; }
}
