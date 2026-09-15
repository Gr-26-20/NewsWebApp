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
        public SmhiWeatherData? SmhiWeather { get; set; }

        public List<WeatherStation> WeatherStations { get; set; } = new();
        
        public List<MunicipalityOption> Municipalities { get; set; } = new()
{
    new() { Name = "Göteborg", ApiValue = "goteborg", Latitude = 57.7089, Longitude = 11.9746 },
    new() { Name = "Stockholm", ApiValue = "stockholm", Latitude = 59.3293, Longitude = 18.0686 },
    new() { Name = "Malmö", ApiValue = "malmo", Latitude = 55.6050, Longitude = 13.0038 },
    new() { Name = "Linköping", ApiValue = "linkoping", Latitude = 58.4108, Longitude = 15.6214 },
    new() { Name = "Örebro", ApiValue = "orebro", Latitude = 59.2753, Longitude = 15.2134 },
    new() { Name = "Uppsala", ApiValue = "uppsala", Latitude = 59.8586, Longitude = 17.6389 },
    new() { Name = "Sundsvall", ApiValue = "sundsvall", Latitude = 62.3908, Longitude = 17.3069 },
    new() { Name = "Östersund", ApiValue = "ostersund", Latitude = 63.1792, Longitude = 14.6357 },
    new() { Name = "Falun", ApiValue = "falun", Latitude = 60.6065, Longitude = 15.6355 },
    new() { Name = "Borlänge", ApiValue = "borlange", Latitude = 60.4858, Longitude = 15.4371 },
    new() { Name = "Kiruna", ApiValue = "kiruna", Latitude = 67.8558, Longitude = 20.2253 }
};

    }
        }

