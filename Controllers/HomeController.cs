using System.Diagnostics;
using HrmApp.Data;
using HrmApp.Models;
using HrmApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HrmDbContext _db;

    public HomeController(UserManager<ApplicationUser> userManager, HrmDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(JobSearchViewModel? model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, DbInitializer.AdminRole))
                    return RedirectToAction("Dashboard", "Admin");
                if (await _userManager.IsInRoleAsync(user, DbInitializer.CompanyRole))
                    return RedirectToAction("Dashboard", "Company");
                if (await _userManager.IsInRoleAsync(user, DbInitializer.CandidateRole))
                    return RedirectToAction("Dashboard", "Candidate");
            }
        }

        model ??= new JobSearchViewModel();
        ViewData["Title"] = "Discover opportunities";
        ViewBag.Cities = StaticOptions.Cities;

        var q = _db.JobPostings
            .AsNoTracking()
            .Include(j => j.Company)
            .Where(j => j.IsActive);

        if (!string.IsNullOrWhiteSpace(model.Query))
        {
            var term = model.Query.Trim();
            q = q.Where(j => j.Title.Contains(term)
                             || j.Description.Contains(term)
                             || j.Company.CompanyName.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(model.Location))
            q = q.Where(j => j.Location == model.Location);

        model.Results = await q.OrderByDescending(j => j.CreatedAt).Take(30).ToListAsync();

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult About()
    {
        ViewData["Title"] = "About us";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Contact()
    {
        ViewData["Title"] = "Contact";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
