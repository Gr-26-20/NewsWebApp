using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Services;

namespace NewsWebApp.Pages;

[Authorize]
public class ProfileModel(UserManager<ApplicationUser> users, ISubscriptionService subscriptions) : PageModel
{
    public ApplicationUser CurrentUser { get; private set; } = null!;
    public Subscriptions? Subscription { get; private set; }
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public class InputModel
    {
        [Required, StringLength(100), Display(Name = "First name")]
        public string FirstName { get; set; } = "";
        [Required, StringLength(100), Display(Name = "Last name")]
        public string LastName { get; set; } = "";
        [Required, Phone, StringLength(100), Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = "";
    }
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        await LoadAsync(user);
        Input = new() { FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber };
        return Page();
    }
    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        await LoadAsync(user);
        if (!ModelState.IsValid) return Page();
        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        user.PhoneNumber = Input.PhoneNumber;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        TempData["Message"] = "Your profile details have been saved.";
        return RedirectToPage();
    }
    private async Task LoadAsync(ApplicationUser user)
    {
        CurrentUser = user;
        Subscription = await subscriptions.GetActiveSubscriptionAsync(user.Id);
    }
}
