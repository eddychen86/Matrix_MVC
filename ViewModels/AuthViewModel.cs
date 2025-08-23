namespace Matrix.ViewModels
{
    /// <summary>
    /// 一般會員註冊 ViewModel - 純數據對象，驗證邏輯完全在 Controller 中處理
    /// </summary>
    public class RegisterViewModel
    {
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string PasswordConfirm { get; set; } = null!;
    }

    /// <summary>
    /// 管理員註冊 ViewModel - 純數據對象，驗證邏輯完全在 Controller 中處理
    /// </summary>
    public class AdminRegisterViewModel
    {
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string PasswordConfirm { get; set; } = null!;
        public int Role { get; set; } = 1;
    }

    /// <summary>
    /// 一般會員＆管理員登入 ViewModel - 純數據對象，驗證邏輯完全在 Controller 中處理
    /// </summary>
    public class LoginViewModel
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } = false;
    }
}