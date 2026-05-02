using System.Diagnostics;
using HrmApp.Data;
using HrmApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrmApp.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
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

        ViewData["Title"] = "Welcome";
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
