using NewsWebApp.Models;

namespace NewsWebApp.Models.ViewModels
{
    public class ForecastViewModel
    {
        public List<WeatherForecast> HourlyForecast { get; set; } = new();

        public List<WeatherForecast> FiveDayForecast { get; set; } = new();
    }
}