using System.ComponentModel.DataAnnotations;

namespace NewsWebApp.Models
{
    public class Users
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } 

        public bool IsSubscribedToNewsletter { get; set; } = false;

        public bool IsSubscribedToNewsNewsletter { get; set; } = false;

        public bool IsSubscribedToWorldNewsletter { get; set; } = false;

        public bool IsSubscribedToSwedenNewsletter { get; set; } = false;

        public bool IsSubscribedToWeatherNewsletter { get; set; } = false;

        public bool IsSubscribedToSportsNewsletter { get; set; } = false;
    }
}


