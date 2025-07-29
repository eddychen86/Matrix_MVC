using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "使用者名稱為必填欄位")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "使用者名稱長度必須介於 3-20 個字元")]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "使用者名稱只能包含字母、數字和底線")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    [StringLength(30, ErrorMessage = "電子郵件長度不可超過 30 個字元")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "密碼為必填欄位")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼長度必須介於 8-20 個字元")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$", 
        ErrorMessage = "密碼必須包含大小寫字母、數字和特殊字元")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "確認密碼為必填欄位")]
    [Compare("Password", ErrorMessage = "確認密碼與密碼不符")]
    public string PasswordConfirm { get; set; } = null!;
  }

  public class LoginViewModel
  {
    [Required(ErrorMessage = "使用者名稱或電子郵件為必填欄位")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "密碼為必填欄位")]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; } = false;
  }
}