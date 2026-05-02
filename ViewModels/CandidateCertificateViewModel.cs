using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class CandidateCertificateViewModel
{
    [Display(Name = "Certificate")]
    public string CertificateName { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Issue date")]
    public string IssueDate { get; set; } = string.Empty;

    [Url]
    [Display(Name = "Certificate link (optional)")]
    public string? CertificateLink { get; set; }
}
