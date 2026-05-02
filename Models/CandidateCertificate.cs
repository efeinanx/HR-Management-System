namespace HrmApp.Models;

public class CandidateCertificate
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string CertificateName { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? CertificateLink { get; set; }
}
