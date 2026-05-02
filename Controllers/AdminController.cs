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

    [HttpGet]
    public async Task<IActionResult> EditJob(int id)
    {
        ViewBag.Section = "jobs";
        ViewBag.Cities = StaticOptions.Cities;
        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();

        var vm = new JobPostingViewModel
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            IsActive = job.IsActive
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(JobPostingViewModel model)
    {
        ViewBag.Section = "jobs";
        ViewBag.Cities = StaticOptions.Cities;
        if (!model.Id.HasValue || !ModelState.IsValid)
            return View(model);

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == model.Id.Value);
        if (job == null) return NotFound();

        job.Title = model.Title.Trim();
        job.Description = model.Description.Trim();
        job.Location = model.Location;
        job.EmploymentType = model.EmploymentType.Trim();
        job.IsActive = model.IsActive;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Job posting updated by admin.";
        return RedirectToAction(nameof(JobAdminDetails), new { id = job.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();
        _db.JobPostings.Remove(job);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Job posting deleted by admin.";
        return RedirectToAction(nameof(AllJobs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null && currentUser.Id == user.Id)
        {
            TempData["Message"] = "You cannot delete your own admin account.";
            return RedirectToAction(nameof(Users));
        }

        var result = await _userManager.DeleteAsync(user);
        TempData["Message"] = result.Succeeded ? "User deleted." : "User could not be deleted.";
        return RedirectToAction(nameof(Users));
    }
}
