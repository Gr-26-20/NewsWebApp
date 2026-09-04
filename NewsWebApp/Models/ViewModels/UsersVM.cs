using NewsWebApp.Data;

namespace NewsWebApp.Models.ViewModels
{
    public class UsersVM
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
