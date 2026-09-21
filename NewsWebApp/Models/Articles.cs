namespace NewsWebApp.Models
{
    public class Articles
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; }
        public bool IsSubscribedUsers { get; set; }
        //public bool IsApproved { get; set; } = false;
        public bool EditorChoice { get; set; }
        public string Category { get; set; }

        public List<Feedback> Feedback { get; set; } = new ();

        public enum Status
        {
            New,
            Pending,
            Approved,
            Rejected,
            Archived

        }

        public Status articleStatus { get; set; } = Status.New;

    }
}
