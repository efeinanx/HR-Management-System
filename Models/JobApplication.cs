namespace HrmApp.Models;

public class JobApplication
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;

    /// <summary>Pending, Approved, Rejected</summary>
    public string Status { get; set; } = "Pending";
    public string? CoverLetter { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
