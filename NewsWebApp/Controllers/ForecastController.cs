using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class ForecastController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly TableStorageService _tableStorageService;

        public ForecastController(
            WeatherService weatherService,
            TableStorageService tableStorageService)
        {
            _weatherService = weatherService;
            _tableStorageService = tableStorageService;
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

        public async Task<IActionResult> History()
        {
            var data =
                await _tableStorageService.GetHistoryAsync();

            var model = data.Select(item => new TemperatureElectricityViewModel
            {
                MeasurementTime = item.MeasurementTime,
                TemperatureC = item.TemperatureC,
                ElectricityPrice = item.ElectricityPrice
            }).ToList();

            return View(model);
        }
    }
}
