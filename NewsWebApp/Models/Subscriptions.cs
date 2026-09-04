namespace NewsWebApp.Models
{
    public class Subscriptions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int BoundingTimeInDays { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public Users UserId { get; set; }
    }
}
