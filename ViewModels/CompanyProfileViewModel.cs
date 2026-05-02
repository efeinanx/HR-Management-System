using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class CompanyProfileViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Industry { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [Url]
    public string? Website { get; set; }
}
