namespace Matrix.ViewModels
{
    /// <summary>
    /// 註冊 ViewModel - 純數據傳輸對象
    /// 驗證規則完全依賴 Program.cs 中的 UserValidationOptions 配置
    /// </summary>
    public class RegisterViewModel
    {
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string PasswordConfirm { get; set; } = null!;
    }

    /// <summary>
    /// 登入 ViewModel - 純數據傳輸對象
    /// </summary>
    public class LoginViewModel
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } = false;
    }
}