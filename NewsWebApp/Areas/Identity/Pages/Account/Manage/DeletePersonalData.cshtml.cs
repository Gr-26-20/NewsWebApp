// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Services;

namespace NewsWebApp.Areas.Identity.Pages.Account.Manage;

public class DeletePersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<DeletePersonalDataModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IStripeBillingGateway _stripe;

    public DeletePersonalDataModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<DeletePersonalDataModel> logger,
        ApplicationDbContext context,
        IStripeBillingGateway stripe)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
        _stripe = stripe;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool RequirePassword { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        if (RequirePassword)
        {
            if (!ModelState.IsValid) return Page();
            if (!await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return Page();
            }
        }

        var userId = await _userManager.GetUserIdAsync(user);

        // Verify recurring billing before removing access to the customer's billing portal.
        var customers = await _context.Subscriptions.Where(s => s.UserId == userId && s.StripeCustomerId != null)
            .Select(s => s.StripeCustomerId!).Distinct().ToListAsync();
        if (!string.IsNullOrWhiteSpace(user.StripeCustomerId)) customers.Add(user.StripeCustomerId);
        try
        {
            foreach (var customerId in customers.Distinct())
            {
                var subscriptions = await _stripe.GetCustomerSubscriptionsAsync(customerId, HttpContext.RequestAborted);
                if (subscriptions.Any(s => s.Status is not ("canceled" or "incomplete_expired") && !s.CancelAtPeriodEnd))
                {
                    ModelState.AddModelError(string.Empty, "Please cancel your recurring subscription in Manage billing before anonymizing your account. Cancellation at the end of your paid period is sufficient.");
                    return Page();
                }
            }
            if (!string.IsNullOrWhiteSpace(user.StripeCheckoutSessionId))
            {
                var session = await _stripe.GetCheckoutAsync(user.StripeCheckoutSessionId, HttpContext.RequestAborted);
                if (session.Status == "open")
                    await _stripe.ExpireCheckoutAsync(session.Id, HttpContext.RequestAborted);
            }
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Could not verify billing before anonymizing user {UserId}.", userId);
            ModelState.AddModelError(string.Empty, "We couldn't verify your billing status. Please try again later.");
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Billing is not configured for account anonymization.");
            ModelState.AddModelError(string.Empty, "We couldn't verify your billing status. Please contact support.");
            return Page();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var anonymousName = $"anonymous-{Guid.NewGuid():N}";
        user.FirstName = "Anonymous";
        user.LastName = "User";
        user.UserName = anonymousName;
        user.Email = $"{anonymousName}@example.invalid";
        user.EmailConfirmed = false;
        user.PhoneNumber = string.Empty;
        // ApplicationUser hides IdentityUser.PhoneNumber; clear both properties.
        ((IdentityUser)user).PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.IsSubscribedToNewsletter = false;
        user.StripeCustomerId = null;
        user.StripeCheckoutSessionId = null;
        user.PasswordHash = null;
        user.TwoFactorEnabled = false;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.AccessFailedCount = 0;
        user.SecurityStamp = Guid.NewGuid().ToString();

        // Clear identity records that can contain personal details or allow another sign-in.
        _context.UserClaims.RemoveRange(await _context.UserClaims.Where(c => c.UserId == userId).ToListAsync());
        _context.UserLogins.RemoveRange(await _context.UserLogins.Where(l => l.UserId == userId).ToListAsync());
        _context.UserTokens.RemoveRange(await _context.UserTokens.Where(t => t.UserId == userId).ToListAsync());
        _context.UserRoles.RemoveRange(await _context.UserRoles.Where(r => r.UserId == userId).ToListAsync());

        // UpdateAsync normalizes the replacement username/email and saves the identity changes.
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Your account could not be anonymized. Please try again.");
            return Page();
        }
        await transaction.CommitAsync();

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User with ID '{UserId}' anonymized their account.", userId);

        return Redirect("~/");
    }
}
