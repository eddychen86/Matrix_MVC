using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Matrix.Models;

public class Person
{
  [Key]
  public Guid PersonId { get; set; }

  // 外鍵屬性，連接到 User
  public Guid? UserId { get; set; }
  
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
  // 一對一關聯到 User
  public virtual User? User { get; set; }
  public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
  public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
  public virtual ICollection<PraiseCollect> PraiseCollects { get; set; } = new List<PraiseCollect>();
  public virtual ICollection<Follow> Follows { get; set; } = new List<Follow>();
  public virtual ICollection<Notification> NotificationsSent { get; set; } = new List<Notification>();
  public virtual ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();
  public virtual ICollection<Report> ReportsMade { get; set; } = new List<Report>();
  public virtual ICollection<Report> ReportsResolved { get; set; } = new List<Report>();
  public virtual ICollection<LoginRecord> LoginRecords { get; set; } = new List<LoginRecord>();
  public virtual ICollection<Friendship> Friends { get; set; } = new List<Friendship>();      // 我加別人
  public virtual ICollection<Friendship> FriendOf { get; set; } = new List<Friendship>();     // 別人加我
}