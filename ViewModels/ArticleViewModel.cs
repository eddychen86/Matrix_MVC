using System;
using Matrix.Models;

namespace Matrix.ViewModels;

public class ArticleViewModel
{
    public required Article Article { get; set; }
    public ArticleAttachment? Attachment { get; set; }
}
