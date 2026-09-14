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

        public List<WeatherStation> WeatherStations { get; set; } = new();
        public List<MunicipalityOption> Municipalities { get; set; } = new()
        {
            new() { Name = "Göteborg", ApiValue = "goteborg" },
            new() { Name = "Stockholm", ApiValue = "stockholm" },
            new() { Name = "Malmö", ApiValue = "malmo" },
            new() { Name = "Linköping", ApiValue = "linkoping" },
            new() { Name = "Örebro", ApiValue = "orebro" },
            new() { Name = "Uppsala", ApiValue = "uppsala" },
            new() { Name = "Sundsvall", ApiValue = "sundsvall" },
            new() { Name = "Östersund", ApiValue = "ostersund" },
            new() { Name = "Falun", ApiValue = "falun" },
            new() { Name = "Borlänge", ApiValue = "borlange" },
            new() { Name = "Kiruna", ApiValue = "kiruna" }
        };

    }
        }

