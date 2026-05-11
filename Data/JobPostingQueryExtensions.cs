using HrmApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Data;

public static class JobPostingQueryExtensions
{
    public static IQueryable<JobPosting> WithKeyword(this IQueryable<JobPosting> query, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return query;

        var term = keyword.Trim().ToLower();
        return query.Where(j =>
            j.Title.ToLower().Contains(term)
            || j.Description.ToLower().Contains(term));
    }
}
