using System.ComponentModel.DataAnnotations;

namespace NewsWebApp.Models
{
    public class NewsLetter
    {
        public int Id { get; set; }

        public string? Logo { get; set; }
        public Category? Category { get; set; }
        public string Email { get; set; } = string.Empty;

        
        public string Subject { get; set; } = string.Empty;

        
        public string Body { get; set; } = string.Empty;

        public string? imageUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? link { get; set; } = string.Empty;

        public string? imageUrl2 { get; set; } = string.Empty;

        public string Title2 { get; set; } = string.Empty;

        public string Description2 { get; set; } = string.Empty;

        public string? link2 { get; set; } = string.Empty;

        public string? imageUrl3 { get; set; } = string.Empty;

        public string Title3 { get; set; } = string.Empty;

        public string Description3 { get; set; } = string.Empty;

        public string link3 { get; set; } = string.Empty;

       
    }
}
