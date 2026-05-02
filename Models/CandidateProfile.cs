namespace HrmApp.Models;

public class CandidateProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
