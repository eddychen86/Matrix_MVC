using System.ComponentModel.DataAnnotations;
using Matrix.Resources.Views.Auth;

namespace Matrix.ViewModels
{
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "用戶名未填寫")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "用戶名需介於 3 到 20 個字，只能包含英文字母、數字和底線")]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "用戶名需介於 3 到 20 個字，只能包含英文字母、數字和底線")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "電子郵件未填寫")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [StringLength(30, ErrorMessage = "電子郵件不得超過 30 個字")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "密碼未填寫")]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼需介於 8 到 20 個字")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
        ErrorMessage = "密碼需至少要有一個大寫字母、一個小寫字母、一個數字、一個特殊符號"
    )]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "確認密碼未填寫")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "密碼不相符")]
    public string PasswordConfirm { get; set; } = null!;
  }

  public class LoginViewModel
  {
    [Required(
        ErrorMessageResourceName = "UserNameInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [StringLength(20,
        MinimumLength = 3,
        ErrorMessageResourceName = "UserNameFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [RegularExpression(
        @"^[a-zA-Z0-9_]{3,20}$",
        ErrorMessageResourceName = "UserNameFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string UserName { get; set; } = null!;

    [Required(
        ErrorMessageResourceName = "PasswordInvalid",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [DataType(DataType.Password)]
    [StringLength(20,
        MinimumLength = 8,
        ErrorMessageResourceName = "PasswordLengthError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
        ErrorMessageResourceName = "PasswordFormatError",
        ErrorMessageResourceType = typeof(Auth)
    )]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }
  }
}