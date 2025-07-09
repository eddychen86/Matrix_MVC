using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class Follow
{
    [Key]
    public Guid FollowId { get; set; }
    public required string UserId { get; set; }
    public Guid IsFollowingId { get; set; }
    public int Type { get; set; }
    public DateTime FollowTime { get; set; }

    [ForeignKey("UserId")]
    public virtual required Person User { get; set; }
    // IsFollowingId 需用商業邏輯處理，不設外鍵
}
