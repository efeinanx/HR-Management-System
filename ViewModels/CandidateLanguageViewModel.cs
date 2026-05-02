using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class CandidateLanguageViewModel
{
    [StringLength(120)]
    public string Language { get; set; } = string.Empty;

    public string Level { get; set; } = "Beginner";
}
