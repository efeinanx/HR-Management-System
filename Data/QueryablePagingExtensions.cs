using Microsoft.EntityFrameworkCore;

namespace HrmApp.Data;

public static class QueryablePagingExtensions
{
    public static async Task<(IList<T> Items, int TotalCount, int Page)> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return (Array.Empty<T>(), 0, 1);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page > totalPages)
            page = totalPages;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, page);
    }
}
