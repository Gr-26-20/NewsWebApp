using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NewsWebApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? StripeCustomerId { get; set; }
    public string? StripeCheckoutSessionId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    
    [Required]
    [StringLength(100)]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsSubscribedToNewsletter { get; set; } = false;

    public bool IsSubscribedToNewsNewsletter { get; set; } = false;


    public bool IsSubscribedToWorldNewsletter { get; set; } = false;

    public bool IsSubscribedToSwedenNewsletter { get; set; } = false;

    public bool IsSubscribedToWeatherNewsletter { get; set; } = false;

    public bool IsSubscribedToSportsNewsletter { get; set; } = false;

}
