namespace Matrix.Services
{
    public class UserValidationOptions
    {
        public UserNameOptions UserName { get; set; } = new();
        public PasswordOptions Password { get; set; } = new();
        public EmailOptions Email { get; set; } = new();
    }

    public class UserNameOptions
    {
        public int RequiredLength { get; set; } = 3;
        public int MaximumLength { get; set; } = 20;
        public string AllowedCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    }

    public class PasswordOptions
    {
        public int RequiredLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 20;
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public int RequiredUniqueChars { get; set; } = 1;
    }

    public class EmailOptions
    {
        public int MaximumLength { get; set; } = 30;
        public bool RequireConfirmedEmail { get; set; } = false;
    }
}