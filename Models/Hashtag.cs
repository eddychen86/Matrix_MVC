using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Hashtag
{
    [Key]
    public Guid TagId { get; set; }
    [Required, MaxLength(10)]
    public required string Content { get; set; }
    public int Status { get; set; } = 0;

    public virtual required ICollection<ArticleHashtag> ArticleHashtags { get; set; }
}
