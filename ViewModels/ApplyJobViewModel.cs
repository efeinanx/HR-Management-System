using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class ApplyJobViewModel
{
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "Cover letter (optional)")]
    public string? CoverLetter { get; set; }
}
