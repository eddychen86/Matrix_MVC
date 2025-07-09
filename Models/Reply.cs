using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Reply
{
    [Key]
    public Guid ReplyId { get; set; }
    [Required]
    public required string UserId { get; set; }
    [Required]
    public required string ArticleId { get; set; }
    [Required, MaxLength(1000)]
    public required string Content { get; set; }
    public DateTime ReplyTime { get; set; }

    [ForeignKey("UserId")]
    public virtual Person? User { get; set; }
    [ForeignKey("ArticleId")]
    public virtual Article? Article { get; set; }
}
