using System.ComponentModel.DataAnnotations;
using HrmApp.Models;

namespace HrmApp.ViewModels;

public class JobSearchViewModel
{
    public const int PageSize = 12;

    [StringLength(200)]
    public string? Query { get; set; }

    [Display(Name = "City")]
    public string? Location { get; set; }

    public int Page { get; set; } = 1;

    public int TotalCount { get; set; }

    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public IList<JobPosting> Results { get; set; } = new List<JobPosting>();
}
