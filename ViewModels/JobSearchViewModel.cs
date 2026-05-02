using System.ComponentModel.DataAnnotations;
using HrmApp.Models;

namespace HrmApp.ViewModels;

public class JobSearchViewModel
{
    [StringLength(200)]
    public string? Query { get; set; }

    [Display(Name = "City")]
    public string? Location { get; set; }

    public IList<JobPosting> Results { get; set; } = new List<JobPosting>();
}
