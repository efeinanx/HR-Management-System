using HrmApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmApp.Controllers;

[AllowAnonymous]
public class CompaniesController : Controller
{
    private readonly HrmDbContext _db;

    public CompaniesController(HrmDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Details(int id)
    {
        var company = await _db.CompanyProfiles
            .AsNoTracking()
            .Include(c => c.JobPostings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        ViewData["Title"] = company.CompanyName;
        return View(company);
    }
}
