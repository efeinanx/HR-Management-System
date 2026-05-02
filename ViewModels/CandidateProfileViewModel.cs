using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HrmApp.ViewModels;

public class CandidateProfileViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Headline { get; set; }

    [StringLength(4000)]
    public string? Summary { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public string? ExistingPhotoPath { get; set; }
    public IFormFile? ProfilePhoto { get; set; }
}
