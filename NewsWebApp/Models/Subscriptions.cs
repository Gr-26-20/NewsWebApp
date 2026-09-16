using NewsWebApp.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsWebApp.Models
{
    public class Subscriptions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int BoundingTimeInDays { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser User { get; set; }
    }
}
