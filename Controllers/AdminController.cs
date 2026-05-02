using HrmApp.Data;
using HrmApp.Models;
using HrmApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
public class AdminController : Controller
{
    private readonly HrmDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(HrmDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "Admin dashboard";
        ViewBag.Section = "dashboard";

        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.CompanyCount = await _db.CompanyProfiles.CountAsync();
        ViewBag.CandidateCount = await _db.CandidateProfiles.CountAsync();
        ViewBag.JobCount = await _db.JobPostings.CountAsync();
        ViewBag.ApplicationCount = await _db.JobApplications.CountAsync();
        ViewBag.AdminIntro = "Cross-tenant metrics (read-only oversight).";

        return View();
    }

    public async Task<IActionResult> Users()
    {
        ViewData["Title"] = "All users";
        ViewData["ListDescription"] = "Registered accounts across the platform.";
        ViewBag.Section = "users";

        var users = await _userManager.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();

        var rows = new List<AdminUserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            rows.Add(new AdminUserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                Roles = string.Join(", ", roles)
            });
        }

        return View(rows);
    }

    public async Task<IActionResult> AllJobs()
    {
        ViewData["Title"] = "All job postings";
        ViewBag.Section = "jobs";

        var jobs = await _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(jobs);
    }

    public async Task<IActionResult> JobAdminDetails(int id)
    {
        ViewBag.Section = "jobs";
        var job = await _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .Include(j => j.Applications)
            .ThenInclude(a => a.Candidate)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        ViewData["Title"] = job.Title;
        return View(job);
    }
}
