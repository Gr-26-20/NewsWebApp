using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public class UserAndRoleService : IUserAndRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserAndRoleService> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAndRoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserAndRoleService> logger, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _httpContextAccessor = httpContext;
        }

        public async Task CreateRole(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        public async Task AddRoleToUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(roleName)
                && !await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
                _logger.LogInformation($"Role '{roleName}' added to user '{user.UserName}'.");
            }
            else
            {
                _logger.LogWarning($"Cannot add role '{roleName}' to user '{user?.UserName ?? "null"}'. Either the user does not exist, the role does not exist, or the user is already in the role.");
            }
        }


        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            return _userManager.Users.ToList();
        }

        public async Task<List<IdentityRole>> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<ApplicationUser>GetCurrentUser()
        {
            var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext?.User);
            if (userId != null)
            {
                return await _userManager.FindByIdAsync(userId);
            }
            return null;
        }
    }
}
