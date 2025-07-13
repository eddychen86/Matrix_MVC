using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Matrix.Models;

public class Person
{
  [Key]
  public required string PersonId { get; set; }

  public int Role { get; set; } = 0;
  public int Status { get; set; } = 0;
  // public required DateTime CreateTime { get; set; }
  [MaxLength(50)]
  public string? DisplayName { get; set; }
  [MaxLength(300)]
  public string? Bio { get; set; }
  [MaxLength(2048)]
  public string? AvatarPath { get; set; }
  [MaxLength(2048)]
  public string? BannerPath { get; set; }
  [MaxLength(2048)]
  public string? ExternalUrl { get; set; }
  public int IsPrivate { get; set; } = 0;
  public string? WalletAddress { get; set; }
  public DateTime? ModifyTime { get; set; }

  // Navigation properties
  [ForeignKey("PersonId")]
  public virtual required IdentityUser User { get; set; }
  public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
  public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
  public virtual ICollection<PraiseCollect> PraiseCollects { get; set; } = new List<PraiseCollect>();
  public virtual ICollection<Follow> Follows { get; set; } = new List<Follow>();
  public virtual ICollection<Notification> NotificationsSent { get; set; } = new List<Notification>();
  public virtual ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();
  public virtual ICollection<Report> ReportsMade { get; set; } = new List<Report>();
  public virtual ICollection<Report> ReportsResolved { get; set; } = new List<Report>();
  public virtual ICollection<LoginRecord> LoginRecords { get; set; } = new List<LoginRecord>();
  public virtual ICollection<Friend.Friends> Friends { get; set; } = new List<Friend.Friends>();      // 我加別人
  public virtual ICollection<Friend.Friends> FriendOf { get; set; } = new List<Friend.Friends>();     // 別人加我
}