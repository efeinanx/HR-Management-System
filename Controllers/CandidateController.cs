using HrmApp.Data;
using HrmApp.Models;
using HrmApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Controllers;

[Authorize(Roles = DbInitializer.CandidateRole)]
public class CandidateController : Controller
{
    private readonly HrmDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CandidateController(HrmDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<CandidateProfile?> GetProfileAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _db.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "Dashboard";
        ViewBag.Section = "dashboard";
        var profile = await GetProfileAsync();
        var applicationCount = profile == null
            ? 0
            : await _db.JobApplications.CountAsync(a => a.CandidateProfileId == profile.Id);

        ViewBag.WelcomeName = profile?.FullName ?? User.Identity?.Name;
        ViewBag.ApplicationCount = applicationCount;
        ViewBag.ActiveJobsCount = await _db.JobPostings.CountAsync(j => j.IsActive);
        ViewData["DashboardSubtitle"] = "Track your job search and applications from this home screen.";

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        ViewData["Title"] = "My profile";
        ViewBag.Section = "profile";
        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var vm = new CandidateProfileViewModel
        {
            FullName = profile.FullName,
            Headline = profile.Headline,
            Summary = profile.Summary,
            Phone = profile.Phone,
            Location = profile.Location
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CandidateProfileViewModel model)
    {
        ViewData["Title"] = "My profile";
        ViewBag.Section = "profile";

        if (!ModelState.IsValid)
            return View(model);

        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        profile.FullName = model.FullName.Trim();
        profile.Headline = model.Headline?.Trim();
        profile.Summary = model.Summary?.Trim();
        profile.Phone = model.Phone?.Trim();
        profile.Location = model.Location?.Trim();

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
            user.DisplayName = model.FullName.Trim();

        await _db.SaveChangesAsync();
        if (user != null)
            await _userManager.UpdateAsync(user);

        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Jobs(JobSearchViewModel model)
    {
        ViewData["Title"] = "Job search";
        ViewBag.Section = "jobs";

        var q = _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .Where(j => j.IsActive);

        if (!string.IsNullOrWhiteSpace(model.Query))
        {
            var term = model.Query.Trim();
            q = q.Where(j => j.Title.Contains(term) || j.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(model.Location))
        {
            var loc = model.Location.Trim();
            q = q.Where(j => j.Location.Contains(loc));
        }

        model.Results = await q.OrderByDescending(j => j.CreatedAt).ToListAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Apply(int id)
    {
        var job = await _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id && j.IsActive);

        if (job == null)
            return NotFound();

        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var exists = await _db.JobApplications.AnyAsync(
            a => a.JobPostingId == id && a.CandidateProfileId == profile.Id);
        if (exists)
        {
            TempData["Message"] = "You have already applied to this job.";
            return RedirectToAction(nameof(Jobs));
        }

        ViewData["Title"] = "Apply";
        ViewBag.Section = "jobs";

        var vm = new ApplyJobViewModel
        {
            JobPostingId = job.Id,
            JobTitle = job.Title,
            CompanyName = job.Company.CompanyName
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyJobViewModel model)
    {
        ViewBag.Section = "jobs";

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Apply";
            return View(model);
        }

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == model.JobPostingId && j.IsActive);
        if (job == null)
            return NotFound();

        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var exists = await _db.JobApplications.AnyAsync(
            a => a.JobPostingId == model.JobPostingId && a.CandidateProfileId == profile.Id);
        if (exists)
        {
            TempData["Message"] = "You have already applied to this job.";
            return RedirectToAction(nameof(Applications));
        }

        _db.JobApplications.Add(new JobApplication
        {
            JobPostingId = model.JobPostingId,
            CandidateProfileId = profile.Id,
            CoverLetter = model.CoverLetter?.Trim(),
            Status = "Pending",
            AppliedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Message"] = "Application submitted.";
        return RedirectToAction(nameof(Applications));
    }

    public async Task<IActionResult> Applications()
    {
        ViewData["Title"] = "My applications";
        ViewBag.Section = "applications";

        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var list = await _db.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPosting)
            .ThenInclude(j => j!.Company)
            .Where(a => a.CandidateProfileId == profile.Id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> ApplicationDetails(int id)
    {
        ViewBag.Section = "applications";
        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var app = await _db.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPosting)
            .ThenInclude(j => j!.Company)
            .FirstOrDefaultAsync(a => a.Id == id && a.CandidateProfileId == profile.Id);

        if (app == null)
            return NotFound();

        ViewData["Title"] = "Application details";
        return View(app);
    }
}
