
namespace NewsWebApp.Models
{
    public class WeatherArticle
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public DateTime ForecastDate { get; set; }

        public string EditorialText { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }
    }
}
