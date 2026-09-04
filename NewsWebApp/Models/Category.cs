namespace NewsWebApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Articles> Articles { get; set; } = new List<Articles>();
    }
}
