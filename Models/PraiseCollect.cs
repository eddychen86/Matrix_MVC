using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class PraiseCollect
{
    [Key]
    public Guid EventId { get; set; }
    public int Type { get; set; }
    public Guid UserId { get; set; }
    public Guid ArticleId { get; set; }
    public DateTime CreateTime { get; set; }

    [ForeignKey("UserId")]
    public virtual Person? User { get; set; }
    [ForeignKey("ArticleId")]
    public virtual Article? Article { get; set; }
}
