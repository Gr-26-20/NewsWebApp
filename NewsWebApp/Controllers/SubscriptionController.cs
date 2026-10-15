using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Data;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers;

[Authorize]
public class SubscriptionController(
    ISubscriptionService subscriptions, StripeBillingService billing,
    UserManager<ApplicationUser> users, ILogger<SubscriptionController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        if (await subscriptions.HasActiveSubscriptionAsync(userId)) return RedirectToAction(nameof(MySubscription));
        return View();
    }
    public IActionResult Pay() => RedirectToAction(nameof(Index));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCheckoutSession(CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        if (await subscriptions.HasActiveSubscriptionAsync(userId)) return RedirectToAction(nameof(MySubscription));
        try { return Redirect(await billing.StartCheckoutAsync(userId, cancellationToken)); }
        catch (Exception ex) when (ex is Stripe.StripeException or InvalidOperationException or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            logger.LogError(ex, "Could not start Stripe checkout for user {UserId}.", userId);
            TempData["BillingError"] = "Checkout could not be started. Please try again shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
    public async Task<IActionResult> Confirmation(string? session_id, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        if (!string.IsNullOrEmpty(session_id))
        {
            try
            {
                if (!await billing.ConfirmCheckoutAsync(session_id, userId, cancellationToken)) return NotFound();
            }
            catch (Stripe.StripeException ex)
            { logger.LogWarning(ex, "Could not verify checkout return for user {UserId}.", userId); }
        }
        ViewBag.SubscriptionActive = await subscriptions.HasActiveSubscriptionAsync(userId);
        return View();
    }
    public IActionResult AlreadySubscribed() => RedirectToAction(nameof(MySubscription));
    public IActionResult PaymentFailed() => View();
    public async Task<IActionResult> MySubscription()
    {
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        var user = await users.GetUserAsync(User);
        ViewBag.HasStripeCustomer = user?.StripeCustomerId is not null;
        return View(await subscriptions.GetActiveSubscriptionAsync(userId));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageBilling(CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        try { return Redirect(await billing.OpenPortalAsync(userId, cancellationToken)); }
        catch (Exception ex) when (ex is Stripe.StripeException or InvalidOperationException)
        {
            logger.LogError(ex, "Could not open Stripe billing portal for user {UserId}.", userId);
            TempData["BillingError"] = "Billing management is temporarily unavailable. Please try again shortly.";
            return RedirectToAction(nameof(MySubscription));
        }
    }
}
