namespace HrmApp.Models;

public class CandidateEducation
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string University { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string StartMonth { get; set; } = string.Empty;
    public string? EndMonth { get; set; }
}
