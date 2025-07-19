using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
  public class LoginViewModel
  {
    [Required]
    [MaxLength(20)]
    public string UserName { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
  }
}

