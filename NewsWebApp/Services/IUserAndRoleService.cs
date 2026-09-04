using NewsWebApp.Data;
using Microsoft.AspNetCore.Identity;

namespace NewsWebApp.Services
{
    public interface IUserAndRoleService
    {
        Task CreateRole(string roleName);
        Task AddRoleToUser(string userId, string roleName);
        Task<List<ApplicationUser>> GetAllUsers();
        Task<List<IdentityRole>> GetAllRoles();
    }
}
