using NewsWebApp.Models;

namespace NewsWebApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Articles> NewsArticles { get; set; } = new();

        public List<Articles> WorldArticles { get; set; } = new();

        public List<Articles> SwedenArticles { get; set; } = new();

        public List<Articles> SportsArticles { get; set; } = new();

        public List<Articles> EditorsChoiceArticles { get; set; } = new();
        public WeatherStation? Weather { get; set; }
        public SmhiWeatherData? SmhiWeather { get; set; }

        public List<WeatherStation> WeatherStations { get; set; } = new();

        public List<MunicipalityOption> Municipalities { get; set; }
            = MunicipalityOption.All;
    }

}