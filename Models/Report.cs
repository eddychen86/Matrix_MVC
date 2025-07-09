using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Report
{
    [Key]
    public Guid ReportId { get; set; }
    public required string ReporterId { get; set; }
    public required string TargetId { get; set; }
    public int Type { get; set; }
    [Required, MaxLength(500)]
    public required string Reason { get; set; }
    public int Status { get; set; } = 0;
    public string? ResolverId { get; set; }
    public DateTime? ProcessTime { get; set; }

    [ForeignKey("ReporterId")]
    public virtual Person? Reporter { get; set; }
    [ForeignKey("ResolverId")]
    public virtual Person? Resolver { get; set; }
    // TargetId 需用商業邏輯處理，不設外鍵
}
