using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
  public class LoginViewModel
  {
    [Required(ErrorMessage = "User name is required")]
    [StringLengthAttribute(20, MinimumLength = 3)]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLengthAttribute(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters long")]
    public string Password { get; set; } = null!;

    public string? ErrorMessage { get; set; }
  }
}