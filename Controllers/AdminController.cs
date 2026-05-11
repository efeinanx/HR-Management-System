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

    public async Task<IActionResult> Users(int page = 1)
    {
        ViewData["Title"] = "Tüm kullanıcılar";
        ViewData["ListDescription"] = "Platformdaki kayıtlı hesaplar.";
        ViewBag.Section = "users";

        var ordered = _userManager.Users.AsNoTracking().OrderBy(u => u.Email);
        var (users, totalCount, currentPage) = await ordered.ToPagedListAsync(
            page,
            AdminPagedViewModel<AdminUserListItemViewModel>.PageSize);

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

        var model = new AdminPagedViewModel<AdminUserListItemViewModel>
        {
            Page = currentPage,
            TotalCount = totalCount,
            Items = rows
        };
        ViewBag.Pagination = BuildPagination(model, nameof(Users));

        return View(model);
    }

    public async Task<IActionResult> UserDetails(string id)
    {
        ViewBag.Section = "users";

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var vm = new AdminUserDetailsViewModel
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Roles = string.Join(", ", roles)
        };

        var company = await _db.CompanyProfiles
            .AsNoTracking()
            .Include(c => c.JobPostings)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (company != null)
        {
            vm.AccountType = "Şirket";
            vm.CompanyProfileId = company.Id;
            vm.CompanyName = company.CompanyName;
            vm.Industry = company.Industry;
            vm.CompanyLocation = company.Location;
            vm.CompanyWebsite = company.Website;
            vm.CompanyDescription = company.Description;
            vm.JobPostingCount = company.JobPostings.Count;
            ViewData["Title"] = company.CompanyName;
            return View(vm);
        }

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (candidate != null)
        {
            vm.AccountType = "Aday";
            vm.CandidateProfileId = candidate.Id;
            vm.CandidateHeadline = candidate.Headline;
            vm.CandidateLocation = candidate.Location;
            vm.CandidateSummary = candidate.Summary;
            vm.ApplicationCount = candidate.Applications.Count;
            ViewData["Title"] = candidate.FullName;
            return View(vm);
        }

        vm.AccountType = roles.Contains(DbInitializer.AdminRole) ? "Yönetici" : "Hesap";
        ViewData["Title"] = user.DisplayName;
        return View(vm);
    }

    public async Task<IActionResult> AllJobs(int page = 1)
    {
        ViewData["Title"] = "Tüm ilanlar";
        ViewBag.Section = "jobs";

        var ordered = _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .OrderByDescending(j => j.CreatedAt);

        var (jobs, totalCount, currentPage) = await ordered.ToPagedListAsync(
            page,
            AdminPagedViewModel<JobPosting>.PageSize);

        var model = new AdminPagedViewModel<JobPosting>
        {
            Page = currentPage,
            TotalCount = totalCount,
            Items = jobs
        };
        ViewBag.Pagination = BuildPagination(model, nameof(AllJobs));

        return View(model);
    }

    private static PagedNavigationViewModel BuildPagination<T>(AdminPagedViewModel<T> model, string actionName) =>
        new()
        {
            Page = model.Page,
            TotalCount = model.TotalCount,
            TotalPages = model.TotalPages,
            ActionName = actionName
        };

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
