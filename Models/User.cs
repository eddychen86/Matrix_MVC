using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matrix.Models;

public class User
{
  [Key]
  public Guid UserId { get; set; }

  // 權限
  public required int Role { get; set; } = 0;

  // 用戶名
  [Required]
  [MaxLength(50)]
  public string UserName { get; set; } = null!;

  // 信箱
  [Required]
  [EmailAddress]
  [MaxLength(100)]
  public string Email { get; set; } = null!;

  // 輸入密碼 (加密)
  [Required]
  public string Password { get; set; } = null!;

  // 確認密碼 (不儲存到資料庫)
  [NotMapped]
  public string? PasswordConfirm { get; set; }

  // 國家
  public string? Country { get; set; }

  // 性別
  public int? Gender { get; set; }

  // 建立帳號時間
  public required DateTime CreateTime { get; set; } = DateTime.Now;

  // 最後登入時間
  public DateTime? LastLoginTime { get; set; }

  // 0: 啟用、1: 停用、2: Ban 
  public int Status { get; set; } = 0;
  
  public virtual Person? Person { get; set; }
}