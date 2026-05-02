namespace HrmApp.Models;

public class CandidateLanguage
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string Language { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner";
}
