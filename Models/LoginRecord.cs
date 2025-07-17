using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class LoginRecord
{
    [Key]
    public Guid LoginId { get; set; }
    public Guid UserId { get; set; }
    public required string IpAddress { get; set; }
    public required string UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
    public required string History { get; set; }

    [ForeignKey("UserId")]
    public virtual required Person User { get; set; }
}
