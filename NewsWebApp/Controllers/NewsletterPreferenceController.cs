using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;

namespace NewsWebApp.Controllers;

[Authorize]
public class NewsletterPreferenceController(UserManager<ApplicationUser> users) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Set(bool enabled)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        user.NewsletterOptIn = enabled;
        var result = await users.UpdateAsync(user);
        TempData["NewsletterPreferenceMessage"] = result.Succeeded
            ? enabled ? "You're signed up for the daily test newsletter." : "You won't receive the daily test newsletter."
            : "Your preference could not be saved. Please try again.";
        return RedirectToAction("Index", "Categories", new { category = "Newsletter" });
    }
}
