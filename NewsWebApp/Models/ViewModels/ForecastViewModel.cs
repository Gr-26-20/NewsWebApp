using NewsWebApp.Models;

namespace NewsWebApp.Models.ViewModels
{
    public class ForecastViewModel
    {
        public List<WeatherForecast> HourlyForecast { get; set; }
            = new();

        public List<WeatherForecast> FiveDayForecast { get; set; }
            = new();

        public string? HourlyForecastError { get; set; }
        public string? FiveDayForecastError { get; set; }

        // Available municipalities for the forecast selector
        public List<MunicipalityOption> Municipalities { get; set; }
            = MunicipalityOption.All;

        // Currently selected municipality
        public string SelectedMunicipality { get; set; } = string.Empty;
    }
}