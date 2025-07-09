using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Notification
{
    [Key]
    public Guid NotifyId { get; set; }
    public required string GetId { get; set; }
    public required string SendId { get; set; }
    public int Type { get; set; }
    public int IsRead { get; set; } = 0;
    public DateTime SentTime { get; set; }
    public DateTime? IsReadTime { get; set; }

    [ForeignKey("GetId")]
    public virtual required Person Receiver { get; set; }
    [ForeignKey("SendId")]
    public virtual required Person Sender { get; set; }
}
