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
                var roleName = roles.FirstOrDefault() ?? string.Empty;
                var userMail = user.Email ?? string.Empty;

                UsersWithRoles.Add(new AddUserToRoleVM
                {
                    UserId = user.Id,
                    RoleName = roleName,
                    Email = userMail
                });
            }

            AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name!).ToList();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            return RedirectToPage();
        }
    }

}