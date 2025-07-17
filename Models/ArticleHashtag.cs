using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class ArticleHashtag
{
    public Guid ArticleId { get; set; }
    public Guid TagId { get; set; }

    [ForeignKey("ArticleId")]
    public virtual Article? Article { get; set; }
    [ForeignKey("TagId")]
    public virtual Hashtag? Hashtag { get; set; }
}
