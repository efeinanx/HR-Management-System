using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HrmApp.ViewModels;

public class ApplyJobViewModel
{
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "Cover letter (optional)")]
    public string? CoverLetter { get; set; }

    [Display(Name = "CV file (PDF/DOC/DOCX)")]
    public IFormFile? CvFile { get; set; }
}
