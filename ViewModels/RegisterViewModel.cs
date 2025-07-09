using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string UserName { get; set; } = null!;
}
