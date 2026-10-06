using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AnalyticsController(AnalyticsService analyticsService) : Controller
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(int days = 7, CancellationToken cancellationToken = default)
    {
        days = days is 7 or 30 or 90 ? days : 7;
        return View(await analyticsService.GetDashboardAsync(days, cancellationToken));
    }
}
