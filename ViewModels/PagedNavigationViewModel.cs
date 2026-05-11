namespace HrmApp.ViewModels;

public class PagedNavigationViewModel
{
    public const int PageSize = 15;

    public int Page { get; set; } = 1;

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public string ActionName { get; set; } = string.Empty;
}
