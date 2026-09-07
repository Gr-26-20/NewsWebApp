using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NewsWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    
    [Required]
    [StringLength(100)]
    public string PhoneNumber { get; set; } = string.Empty;
}
