using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.Build.Framework;

namespace NewsWebApp.Models
{
    public class Articles 
    {
        public int Id { get; set; }
        //[Required]
        public string Slug { get; set; }
        
        public string Title { get; set; }
        //[Required]
        public string Author { get; set; }
        //[Required]
        public string Summary { get; set; }
        //[Required]
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        //[Required]
        public int Views { get; set; }
        //[Required]
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; }
        public bool IsSubscribedUsers { get; set; }
        //public bool IsApproved { get; set; } = false;
        public bool EditorChoice { get; set; }
        public string Category { get; set; }

       


        public IList<Feedback> Feedback { get; set; } = new List<Feedback>();

        public enum Status
        {
            [Display(Name = "New")]
            New,
            [Display(Name = "Pending")]
            Pending,
            [Display(Name = "Approved")]
            Approved,
            [Display(Name = "Rejected")]
            Rejected,
            [Display(Name = "Archived")]
            Archived,
            
        }

        public Status articleStatus { get; set; }
        //public bool IsArchived { get; set; } = false;
        //public bool EditorChoice { get; set; } = false;
        //[Required]
        //public string Category { get; set; }
        //public bool IsSubscribedUsers { get; set; } = false;
    }
}
