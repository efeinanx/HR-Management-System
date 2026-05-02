namespace HrmApp.Models;

public class CompanyProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? ProfilePhotoPath { get; set; }

    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
