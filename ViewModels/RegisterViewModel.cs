using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class RegisterViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an account type.")]
    [RegularExpression("^(Company|Candidate)$", ErrorMessage = "Invalid account type.")]
    [Display(Name = "I am registering as")]
    public string Role { get; set; } = "Candidate";
}
