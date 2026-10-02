using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models;
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

            var model = new ForecastViewModel
            {
                HourlyForecast = new List<WeatherForecast>(),
                FiveDayForecast = new List<WeatherForecast>()
            };

            try
            {
                // TEST: simulate 24-hour weather API failure by uncommenting next line
                // throw new HttpRequestException("Test: 24-hour weather API is unavailable.");
                model.HourlyForecast =
                    await _weatherService.Get24HourForecastAsync(municipality);
            }
            catch (HttpRequestException)
            {
                model.HourlyForecastError =
                    "24-hour forecast is currently unavailable. Please try again later.";
            }

            try
            {
                // TEST: simulate 5-day weather API failure
                // throw new HttpRequestException("Test: 5-day weather API is unavailable.");
                model.FiveDayForecast =
                    await _weatherService.Get5DayForecastAsync(municipality);
            }
            catch (HttpRequestException)
            {
                model.FiveDayForecastError =
                    "5-day forecast is currently unavailable. Please try again later.";
            }

            return View(model);
        }

        public async Task<IActionResult> History()
        {
            try
            {
                // throw new Exception("Test: Azure Table Storage is unavailable.");

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
            catch (Exception)
            {
                ViewData["HistoryError"] =
                    "Historical weather and/or electricity data is currently unavailable. Please try again later.";

                return View(new List<TemperatureElectricityViewModel>());
            }
        }
    }
}
