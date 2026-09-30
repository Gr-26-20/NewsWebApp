using NewsWebApp.Models;

namespace NewsWebApp.Models.ViewModels
{
    public class ForecastViewModel
    {
        public List<WeatherForecast> HourlyForecast { get; set; }
        public List<WeatherForecast> FiveDayForecast { get; set; }

        public string? HourlyForecastError { get; set; }
        public string? FiveDayForecastError { get; set; }
    }
}

