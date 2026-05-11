namespace HrmApp.ViewModels;

public class AdminPagedViewModel<T>
{
    public const int PageSize = 15;

    public int Page { get; set; } = 1;

    public int TotalCount { get; set; }

    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public IList<T> Items { get; set; } = new List<T>();
}
