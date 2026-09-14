using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWebApp.Data;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Models;



namespace NewsWebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public List<AddUserToRoleVM> UsersWithRoles { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Load all users
            var users = await _context.Users.ToListAsync();

            // Get roles for each user using UserManager
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userMail = user.Email ?? string.Empty;

                UsersWithRoles.Add(new AddUserToRoleVM
                {
                    UserId = user.Id,
                    RoleNames = roles.ToList(), // Convert to List<string>
                    Email = userMail
                });
            }

            AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name!).ToList();
        }

        // Add role to user
        public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "User and Role are required.";
                return RedirectToPage();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToPage();
                }

                var hasRole = await _userManager.IsInRoleAsync(user, role);
                if (hasRole)
                {
                    TempData["InfoMessage"] = $"User already has the '{role}' role.";
                    return RedirectToPage();
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{role}' added successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Remove role from user
        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "User and Role are required.";
                return RedirectToPage();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToPage();
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{role}' removed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}