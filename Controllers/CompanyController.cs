using HrmApp.Data;
using HrmApp.Models;
using HrmApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Controllers;

[Authorize(Roles = DbInitializer.CompanyRole)]
public class CompanyController : Controller
{
    private readonly HrmDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyController(HrmDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<CompanyProfile?> GetProfileAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _db.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "Dashboard";
        ViewBag.Section = "dashboard";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        ViewBag.CompanyName = company.CompanyName;

        var jobIds = _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id);

        ViewBag.ActiveJobs = await _db.JobPostings.CountAsync(j => j.CompanyProfileId == company.Id && j.IsActive);
        ViewBag.TotalApplications = await _db.JobApplications.CountAsync(a => jobIds.Contains(a.JobPostingId));
        ViewBag.PendingApplications = await _db.JobApplications.CountAsync(a =>
            jobIds.Contains(a.JobPostingId) && a.Status == "Pending");
        ViewBag.ApprovedApplications = await _db.JobApplications.CountAsync(a =>
            jobIds.Contains(a.JobPostingId) && a.Status == "Approved");

        ViewData["CompanyDashboardHint"] = "High-level recruiting metrics for your organization.";

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        ViewData["Title"] = "Company profile";
        ViewBag.Section = "profile";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var vm = new CompanyProfileViewModel
        {
            CompanyName = company.CompanyName,
            Industry = company.Industry,
            Description = company.Description,
            Website = company.Website
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CompanyProfileViewModel model)
    {
        ViewData["Title"] = "Company profile";
        ViewBag.Section = "profile";

        if (!ModelState.IsValid)
            return View(model);

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        company.CompanyName = model.CompanyName.Trim();
        company.Industry = model.Industry?.Trim();
        company.Description = model.Description?.Trim();
        company.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
            user.DisplayName = model.CompanyName.Trim();

        await _db.SaveChangesAsync();
        if (user != null)
            await _userManager.UpdateAsync(user);

        TempData["Message"] = "Company profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> JobPostings()
    {
        ViewData["Title"] = "My job postings";
        ViewBag.Section = "jobs";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var list = await _db.JobPostings
            .AsNoTracking()
            .Where(j => j.CompanyProfileId == company.Id)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateJob()
    {
        ViewData["Title"] = "Create job posting";
        ViewBag.Section = "jobs";
        return View(new JobPostingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(JobPostingViewModel model)
    {
        ViewBag.Section = "jobs";

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create job posting";
            return View(model);
        }

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var job = new JobPosting
        {
            CompanyProfileId = company.Id,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Location = model.Location.Trim(),
            EmploymentType = model.EmploymentType.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.JobPostings.Add(job);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Job posting created.";
        return RedirectToAction(nameof(JobDetails), new { id = job.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditJob(int id)
    {
        ViewBag.Section = "jobs";
        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == id && j.CompanyProfileId == company.Id);
        if (job == null)
            return NotFound();

        ViewData["Title"] = "Edit job posting";

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

        if (!model.Id.HasValue || !ModelState.IsValid)
        {
            ViewData["Title"] = "Edit job posting";
            return View(model);
        }

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == model.Id && j.CompanyProfileId == company.Id);
        if (job == null)
            return NotFound();

        job.Title = model.Title.Trim();
        job.Description = model.Description.Trim();
        job.Location = model.Location.Trim();
        job.EmploymentType = model.EmploymentType.Trim();
        job.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Message"] = "Job posting updated.";
        return RedirectToAction(nameof(JobDetails), new { id = job.Id });
    }

    public async Task<IActionResult> JobDetails(int id)
    {
        ViewBag.Section = "jobs";
        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var job = await _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Applications)
            .ThenInclude(a => a.Candidate)
            .FirstOrDefaultAsync(j => j.Id == id && j.CompanyProfileId == company.Id);

        if (job == null)
            return NotFound();

        ViewData["Title"] = job.Title;
        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == id && j.CompanyProfileId == company.Id);
        if (job == null)
            return NotFound();

        _db.JobPostings.Remove(job);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Job posting deleted.";
        return RedirectToAction(nameof(JobPostings));
    }

    public async Task<IActionResult> CandidatePool()
    {
        ViewData["Title"] = "Candidate pool";
        ViewBag.Section = "pool";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var jobIds = await _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id).ToListAsync();

        var apps = await _db.JobApplications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .Where(a => jobIds.Contains(a.JobPostingId))
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(apps);
    }

    public async Task<IActionResult> PendingApplications()
    {
        ViewData["Title"] = "Pending applications";
        ViewBag.Section = "pending";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var jobIds = await _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id).ToListAsync();

        var apps = await _db.JobApplications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .Where(a => jobIds.Contains(a.JobPostingId) && a.Status == "Pending")
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(apps);
    }

    public async Task<IActionResult> ApprovedCandidates()
    {
        ViewData["Title"] = "Approved candidates";
        ViewBag.Section = "approved";

        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var jobIds = await _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id).ToListAsync();

        var apps = await _db.JobApplications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .Where(a => jobIds.Contains(a.JobPostingId) && a.Status == "Approved")
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(apps);
    }

    public async Task<IActionResult> ApplicationReview(int id)
    {
        ViewBag.Section = "pool";
        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var jobIds = await _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id).ToListAsync();

        var app = await _db.JobApplications
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == id && jobIds.Contains(a.JobPostingId));

        if (app == null)
            return NotFound();

        ViewData["Title"] = "Review application";

        var vm = new UpdateApplicationStatusViewModel
        {
            ApplicationId = app.Id,
            Status = app.Status
        };

        ViewBag.Application = app;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplicationReview(UpdateApplicationStatusViewModel model)
    {
        var company = await GetProfileAsync();
        if (company == null)
            return NotFound();

        var jobIds = await _db.JobPostings.Where(j => j.CompanyProfileId == company.Id).Select(j => j.Id).ToListAsync();

        var app = await _db.JobApplications
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == model.ApplicationId && jobIds.Contains(a.JobPostingId));

        if (app == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Review application";
            ViewBag.Section = "pool";
            ViewBag.Application = app;
            return View(model);
        }

        app.Status = model.Status;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Application status updated.";
        return RedirectToAction(nameof(ApplicationReview), new { id = app.Id });
    }
}
