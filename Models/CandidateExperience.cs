namespace HrmApp.Models;

public class CandidateExperience
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string StartMonth { get; set; } = string.Empty;
    public string? EndMonth { get; set; }
    public string? Description { get; set; }
}
