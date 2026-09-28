namespace NewsWebApp.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        //public int ArticleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //public Articles? Article { get; set; }

        public required int ArticleId { get; set; }

        //public required Articles Article { get; set; }

        public string Feedbackstring { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}
