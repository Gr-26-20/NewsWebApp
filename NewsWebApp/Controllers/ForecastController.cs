using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using Stripe;

namespace NewsWebApp.Controllers
{
    public class ForecastController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly TableStorageService _tableStorageService;
        private readonly WeatherEditorialAiService _weatherEditorialAiService;
        private readonly WeatherImageAiService _weatherImageAiService;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;
        private readonly SmhiWeatherWarningService _smhiWeatherWarningService;
        private readonly WeatherArticleGenerationService
            _weatherArticleGenerationService;
        




        public ForecastController(
        WeatherService weatherService,
        TableStorageService tableStorageService,
        WeatherEditorialAiService weatherEditorialAiService,
        WeatherImageAiService weatherImageAiService,
        IFileService fileService,
        ApplicationDbContext context,
        SmhiWeatherWarningService smhiWeatherWarningService,
        WeatherArticleGenerationService weatherArticleGenerationService)
        {
            _weatherService = weatherService;
            _tableStorageService = tableStorageService;
            _weatherEditorialAiService = weatherEditorialAiService;
            _weatherImageAiService = weatherImageAiService;
            _smhiWeatherWarningService = smhiWeatherWarningService;
            _weatherArticleGenerationService = weatherArticleGenerationService;
            _fileService = fileService;
            _context = context;
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


        [HttpGet]
        public async Task<IActionResult> TestWeatherEditorial(
            CancellationToken cancellationToken)
        {
            var regionalForecasts = await Task.WhenAll(
                _weatherService.Get24HourForecastAsync("Gothenburg"),
                _weatherService.Get24HourForecastAsync("Stockholm"),
                _weatherService.Get24HourForecastAsync("Umea")
            );

            var forecast = regionalForecasts
                .SelectMany(region => region)
                .ToList();

            var warnings =
                await _smhiWeatherWarningService.GetActiveWarningsAsync();

            Console.WriteLine($"Regional forecast records: {forecast.Count}");
            Console.WriteLine($"SMHI warning count: {warnings.Count}");

            var editorial =
                await _weatherEditorialAiService.GenerateEditorialAsync(
                    "Sweden",
                    forecast,
                    warnings,
                    cancellationToken);

            return Content(editorial, "text/plain; charset=utf-8");
        }




        public async Task<IActionResult> TestFiveLocationForecasts()
        {
            var locations = new[]
            {
                "Malmo",
                "Gothenburg",
                "Stockholm",
                "Umea",
                "Are"
            };

            var results = await Task.WhenAll(
                locations.Select(async location => new
                {
                    Location = location,
                    Forecast = await _weatherService
                        .Get24HourForecastAsync(location)
                }));

            var summary = results.Select(result => new
            {
                result.Location,
                ForecastCount = result.Forecast.Count,
                FirstForecast = result.Forecast.FirstOrDefault()
            });

            return Json(summary);
        }






        [HttpGet]
        public async Task<IActionResult> TestWeatherImage(
            CancellationToken cancellationToken)
        {
            var article = await _weatherArticleGenerationService
                .GenerateAndSaveAsync(cancellationToken);

            return Content(
                $"Weather article saved successfully.\n" +
                $"City: {article.City}\n" +
                $"Article ID: {article.Id}\n" +
                $"Image URL: {article.ImageUrl}",
                "text/plain");
        }


        [HttpGet]
        public async Task<IActionResult> TestSavedWeatherArticle(
            CancellationToken cancellationToken)
        {
            var article = await _context.WeatherArticles
                .OrderByDescending(a => a.GeneratedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (article == null)
            {
                return NotFound("No saved weather articles were found.");
            }

            return Content(
                $"City: {article.City}\n" +
                $"Forecast date: {article.ForecastDate:yyyy-MM-dd}\n" +
                $"Generated at: {article.GeneratedAt:u}\n\n" +
                $"Editorial:\n{article.EditorialText}\n\n" +
                $"Image URL:\n{article.ImageUrl}",
                "text/plain");
        }






    }


}
