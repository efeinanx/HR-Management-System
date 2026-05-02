using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class CandidateExperienceViewModel
{
    [Display(Name = "Company")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string Position { get; set; } = string.Empty;

    [Display(Name = "Start month")]
    public string StartMonth { get; set; } = string.Empty;

    [Display(Name = "End month")]
    public string? EndMonth { get; set; }

    [StringLength(1500)]
    public string? Description { get; set; }
}
