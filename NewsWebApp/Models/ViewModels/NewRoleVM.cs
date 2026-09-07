using System.ComponentModel;

namespace NewsWebApp.Models.ViewModels
{
    public class NewRoleVM
    {

        [DisplayName("Role Name")]
        public string RoleName { get; set; } = string.Empty;
    }
}
