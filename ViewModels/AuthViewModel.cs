using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
  public class LoginViewModel
  {
    [Required(ErrorMessage = "請輸入使用者名稱")]
    [MaxLength(20)]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
  }
}

