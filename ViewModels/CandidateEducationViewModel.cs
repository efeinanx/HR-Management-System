using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class CandidateEducationViewModel
{
    public string University { get; set; } = string.Empty;
    public string? OtherUniversity { get; set; }

    public string Faculty { get; set; } = string.Empty;

    [StringLength(200)]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Start month")]
    public string StartMonth { get; set; } = string.Empty;

    [Display(Name = "End month")]
    public string? EndMonth { get; set; }
}
