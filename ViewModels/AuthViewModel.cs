using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }

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