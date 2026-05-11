namespace HrmApp.ViewModels;

public class AdminUserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public int? CompanyProfileId { get; set; }
    public int? CandidateProfileId { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? CompanyLocation { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyDescription { get; set; }
    public string? CandidateHeadline { get; set; }
    public string? CandidateLocation { get; set; }
    public string? CandidateSummary { get; set; }
    public int JobPostingCount { get; set; }
    public int ApplicationCount { get; set; }
}
