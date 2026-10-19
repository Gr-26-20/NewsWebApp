
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NewsWebApp.Services;
using System.Security.Claims;

namespace NewsWebApp.ViewComponents;

public sealed class AdvertisementViewComponent(
    ISubscriptionService subscriptions) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is not null &&
            await subscriptions.HasActiveSubscriptionAsync(userId))
        {
            return Content(string.Empty);
        }

        return View();
    }
}
