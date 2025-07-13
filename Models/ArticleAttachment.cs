using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class ArticleAttachment
{
    [Key]
    public Guid FileId { get; set; }

    [Required]
    public string ArticleId { get; set; } = "";

    [Required]
    public string FilePath { get; set; } = "";

    [Required]
    public string Type { get; set; } = ""; // "image" 或 "file"

    public string? FileName { get; set; }
    public string? MimeType { get; set; }

    // 導覽屬性
    public virtual Article? Article { get; set; }
}
