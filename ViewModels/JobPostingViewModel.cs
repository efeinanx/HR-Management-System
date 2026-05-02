using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class JobPostingViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(8000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "City")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Employment type")]
    public string EmploymentType { get; set; } = "Full-time";

    [Display(Name = "Active listing")]
    public bool IsActive { get; set; } = true;
}
