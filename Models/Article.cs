using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Article
{
    [Key]
    public required string ArticleId { get; set; }
    [Required]
    public required string AuthorId { get; set; }
    [Required, MaxLength(4000)]
    public required string Content { get; set; }
    public int IsPublic { get; set; } = 0;
    public int Status { get; set; } = 0;
    public DateTime CreateTime { get; set; }
    public int PraiseCount { get; set; } = 0;
    public int CollectCount { get; set; } = 0;

    // Navigation properties
    public virtual Person? Author { get; set; }
    public virtual ICollection<ArticleHashtag>? ArticleHashtags { get; set; }
    public virtual ICollection<Reply>? Replies { get; set; }
    public virtual ICollection<PraiseCollect>? PraiseCollects { get; set; }
    public virtual ICollection<ArticleAttachment>? Attachments { get; set; }
}
