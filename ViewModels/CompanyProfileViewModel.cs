using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

    [Required]
    [Display(Name = "City")]
    public string Location { get; set; } = string.Empty;

    public string? ExistingPhotoPath { get; set; }
    [Display(Name = "Company logo")]
    public IFormFile? ProfilePhoto { get; set; }
}
