using Microsoft.AspNetCore.Identity;

namespace HrmApp.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public CompanyProfile? CompanyProfile { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
}
