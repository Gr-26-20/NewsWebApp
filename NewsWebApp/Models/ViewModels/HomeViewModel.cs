using NewsWebApp.Models;

namespace NewsWebApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Articles> NewsArticles { get; set; } = new();

        public List<Articles> WorldArticles { get; set; } = new();

        public List<Articles> SwedenArticles { get; set; } = new();

        public List<Articles> SportsArticles { get; set; } = new();

        public WeatherStation? Weather { get; set; }
    }
}
