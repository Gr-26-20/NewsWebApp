namespace NewsWebApp.Models.ViewModels
{
    public class AddUserToRoleVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> RoleNames { get; set; } = new(); // changed to list of strings to hold role names
    }
}
