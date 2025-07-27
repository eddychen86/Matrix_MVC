using System.ComponentModel.DataAnnotations;
using Matrix.Resources.Views.Auth;

namespace Matrix.ViewModels
{
  public class RegisterViewModel
  {
    [Required(
        ErrorMessageResourceName = "UserNameInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [StringLength(20, MinimumLength = 3,
        ErrorMessageResourceName = "UserNameFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$",
        ErrorMessageResourceName = "UserNameFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string UserName { get; set; } = null!;

    [Required(
        ErrorMessageResourceName = "EmailRequired",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [EmailAddress(
        ErrorMessageResourceName = "EmailInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [StringLength(30,
        ErrorMessageResourceName = "EmailFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string? Email { get; set; }

    [Required(
        ErrorMessageResourceName = "PasswordInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 8,
        ErrorMessageResourceName = "PasswordLengthError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,20}$",
        ErrorMessageResourceName = "PasswordFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string Password { get; set; } = null!;

    [Required(
        ErrorMessageResourceName = "PasswordConfirmRequired",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [DataType(DataType.Password)]
    [Compare("Password",
        ErrorMessageResourceName = "PasswordCompareError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string PasswordConfirm { get; set; } = null!;
  }

  public class LoginViewModel
  {
    [Required(
        ErrorMessageResourceName = "UserNameInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [StringLength(20,
        ErrorMessageResourceName = "UserNameLengthError", 
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string UserName { get; set; } = null!;

    [Required(
        ErrorMessageResourceName = "PasswordInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [DataType(DataType.Password)]
    [StringLength(20,
        ErrorMessageResourceName = "PasswordLengthError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }
  }
}