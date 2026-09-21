using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class ForecastController : Controller
    {
        private readonly WeatherService _weatherService;

        public ForecastController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index()
        {
            var municipality =
                HttpContext.Session.GetString("SelectedWeatherMunicipality")
                ?? "Gothenburg";

            var hourlyForecast =
                await _weatherService.Get24HourForecastAsync(municipality);

            var fiveDayForecast =
                await _weatherService.Get5DayForecastAsync(municipality);

            var model = new ForecastViewModel
            {
                HourlyForecast = hourlyForecast,
                FiveDayForecast = fiveDayForecast
            };

            return View(model);
        }
    }
}
