namespace NewsWebApp.Models
{
    public class Articles
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsAcrhived { get; set; }
        public string Category { get; set; }
        public string CreatedByUserId { get; set; }

    }
}
