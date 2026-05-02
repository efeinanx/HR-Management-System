using System.ComponentModel.DataAnnotations;

namespace HrmApp.ViewModels;

public class UpdateApplicationStatusViewModel
{
    public int ApplicationId { get; set; }

    [Required]
    [RegularExpression("^(Pending|Approved|Rejected)$", ErrorMessage = "Status must be Pending, Approved, or Rejected.")]
    public string Status { get; set; } = "Pending";
}
