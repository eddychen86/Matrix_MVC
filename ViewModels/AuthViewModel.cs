using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(30, ErrorMessage = "Email must be less than 30 characters")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)")]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; set; } = null!;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)")]
        public string Password { get; set; } = null!;

        public string? ErrorMessage { get; set; }
    }
}