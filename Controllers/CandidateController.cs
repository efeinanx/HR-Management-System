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
    private readonly IWebHostEnvironment _env;

    public CandidateController(HrmDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    private async Task<CandidateProfile?> GetProfileAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _db.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
    }

    private void FillStaticDropdowns()
    {
        ViewBag.Cities = StaticOptions.Cities;
        ViewBag.Universities = StaticOptions.Universities;
        ViewBag.Faculties = StaticOptions.Faculties;
        ViewBag.LanguageLevels = StaticOptions.LanguageLevels;
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
        FillStaticDropdowns();

        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        var vm = new CandidateProfilePageViewModel
        {
            Profile = new CandidateProfileViewModel
            {
                FullName = profile.FullName,
                Headline = profile.Headline,
                Summary = profile.Summary,
                Phone = profile.Phone,
                Location = profile.Location,
                ExistingPhotoPath = profile.ProfilePhotoPath
            },
            Educations = await _db.CandidateEducations.Where(x => x.CandidateProfileId == profile.Id).OrderByDescending(x => x.StartMonth).ToListAsync(),
            Experiences = await _db.CandidateExperiences.Where(x => x.CandidateProfileId == profile.Id).OrderByDescending(x => x.StartMonth).ToListAsync(),
            Languages = await _db.CandidateLanguages.Where(x => x.CandidateProfileId == profile.Id).OrderBy(x => x.Language).ToListAsync(),
            Certificates = await _db.CandidateCertificates.Where(x => x.CandidateProfileId == profile.Id).OrderByDescending(x => x.Year).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] CandidateProfileViewModel model)
    {
        ViewData["Title"] = "My profile";
        ViewBag.Section = "profile";
        FillStaticDropdowns();

        if (!ModelState.IsValid)
            return await Profile();
        var profile = await GetProfileAsync();
        if (profile == null)
            return NotFound();

        profile.FullName = model.FullName.Trim();
        profile.Headline = model.Headline?.Trim();
        profile.Summary = model.Summary?.Trim();
        profile.Phone = model.Phone?.Trim();
        profile.Location = model.Location?.Trim();

        if (model.ProfilePhoto is { Length: > 0 })
            profile.ProfilePhotoPath = await SaveFileAsync(model.ProfilePhoto, "uploads/photos");

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
            user.DisplayName = model.FullName.Trim();

        await _db.SaveChangesAsync();
        if (user != null)
            await _userManager.UpdateAsync(user);

        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEducation([Bind(Prefix = "NewEducation")] CandidateEducationViewModel e)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        var university = e.University == "Other" ? e.OtherUniversity : e.University;
        if (string.IsNullOrWhiteSpace(university) || string.IsNullOrWhiteSpace(e.Faculty) || string.IsNullOrWhiteSpace(e.Department) || string.IsNullOrWhiteSpace(e.StartMonth))
        {
            TempData["Message"] = "Education fields are required.";
            return RedirectToAction(nameof(Profile));
        }

        _db.CandidateEducations.Add(new CandidateEducation
        {
            CandidateProfileId = profile.Id,
            University = university!.Trim(),
            Faculty = e.Faculty,
            Department = e.Department.Trim(),
            StartMonth = e.StartMonth,
            EndMonth = e.EndMonth
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Education added.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExperience([Bind(Prefix = "NewExperience")] CandidateExperienceViewModel x)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        if (string.IsNullOrWhiteSpace(x.CompanyName) || string.IsNullOrWhiteSpace(x.Position) || string.IsNullOrWhiteSpace(x.StartMonth))
        {
            TempData["Message"] = "Experience fields are required.";
            return RedirectToAction(nameof(Profile));
        }

        _db.CandidateExperiences.Add(new CandidateExperience
        {
            CandidateProfileId = profile.Id,
            CompanyName = x.CompanyName.Trim(),
            Position = x.Position.Trim(),
            StartMonth = x.StartMonth,
            EndMonth = x.EndMonth,
            Description = x.Description?.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Experience added.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLanguage([Bind(Prefix = "NewLanguage")] CandidateLanguageViewModel l)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        if (string.IsNullOrWhiteSpace(l.Language))
        {
            TempData["Message"] = "Language is required.";
            return RedirectToAction(nameof(Profile));
        }

        _db.CandidateLanguages.Add(new CandidateLanguage
        {
            CandidateProfileId = profile.Id,
            Language = l.Language.Trim(),
            Level = l.Level
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Language added.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCertificate([Bind(Prefix = "NewCertificate")] CandidateCertificateViewModel c)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        if (string.IsNullOrWhiteSpace(c.CertificateName) || string.IsNullOrWhiteSpace(c.Issuer) || string.IsNullOrWhiteSpace(c.IssueDate))
        {
            TempData["Message"] = "Certificate fields are required.";
            return RedirectToAction(nameof(Profile));
        }

        if (!DateTime.TryParse(c.IssueDate, out var issueDate))
        {
            TempData["Message"] = "Invalid certificate date.";
            return RedirectToAction(nameof(Profile));
        }

        _db.CandidateCertificates.Add(new CandidateCertificate
        {
            CandidateProfileId = profile.Id,
            CertificateName = c.CertificateName.Trim(),
            Issuer = c.Issuer.Trim(),
            Year = issueDate.Year,
            CertificateLink = c.CertificateLink?.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Certificate added.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        var row = await _db.CandidateEducations.FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
        if (row == null) return NotFound();
        _db.CandidateEducations.Remove(row);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExperience(int id)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        var row = await _db.CandidateExperiences.FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
        if (row == null) return NotFound();
        _db.CandidateExperiences.Remove(row);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        var row = await _db.CandidateLanguages.FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
        if (row == null) return NotFound();
        _db.CandidateLanguages.Remove(row);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCertificate(int id)
    {
        var profile = await GetProfileAsync();
        if (profile == null) return NotFound();
        var row = await _db.CandidateCertificates.FirstOrDefaultAsync(x => x.Id == id && x.CandidateProfileId == profile.Id);
        if (row == null) return NotFound();
        _db.CandidateCertificates.Remove(row);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Jobs(JobSearchViewModel model)
    {
        ViewData["Title"] = "Job search";
        ViewBag.Section = "jobs";
        ViewBag.Cities = StaticOptions.Cities;

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
            q = q.Where(j => j.Location == model.Location);

        model.Results = await q.OrderByDescending(j => j.CreatedAt).ToListAsync();

        var profile = await GetProfileAsync();
        ViewBag.AppliedJobIds = profile == null
            ? new HashSet<int>()
            : await _db.JobApplications
                .Where(a => a.CandidateProfileId == profile.Id)
                .Select(a => a.JobPostingId)
                .ToHashSetAsync();

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

        var cvPath = model.CvFile is { Length: > 0 }
            ? await SaveFileAsync(model.CvFile, "uploads/cv")
            : null;

        _db.JobApplications.Add(new JobApplication
        {
            JobPostingId = model.JobPostingId,
            CandidateProfileId = profile.Id,
            CoverLetter = model.CoverLetter?.Trim(),
            CvFilePath = cvPath,
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

    private async Task<string> SaveFileAsync(IFormFile file, string folderRelative)
    {
        var root = _env.WebRootPath;
        var directory = Path.Combine(root, folderRelative);
        Directory.CreateDirectory(directory);

        var ext = Path.GetExtension(file.FileName);
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(directory, safeName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);
        return "/" + folderRelative.Replace("\\", "/") + "/" + safeName;
    }
}
