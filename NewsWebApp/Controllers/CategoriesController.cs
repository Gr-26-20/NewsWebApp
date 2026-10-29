using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Models;
using NewsWebApp.Services;
using NewsWebApp.Data;

namespace NewsWebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly WeatherService _weatherService;
        private readonly WeatherEditorialAiService _weatherEditorialAiService;
        private readonly ILogger<CategoriesController> _logger;
        private readonly ApplicationDbContext _context;    
        private readonly WeatherImageAiService _weatherImageAiService;
        private readonly SmhiWeatherWarningService _smhiWeatherWarningService;
        private readonly IFileService _fileService;

        public CategoriesController(
            IArticleService articleService,
            WeatherService weatherService,
            WeatherEditorialAiService weatherEditorialAiService,
            ILogger<CategoriesController> logger,
            ApplicationDbContext context,
            WeatherImageAiService weatherImageAiService,
            SmhiWeatherWarningService smhiWeatherWarningService,
            IFileService fileService)
        {
            _articleService = articleService;
            _weatherService = weatherService;
            _weatherEditorialAiService = weatherEditorialAiService;
            _logger = logger;
            _context = context;
            _weatherImageAiService = weatherImageAiService;
            _smhiWeatherWarningService = smhiWeatherWarningService;
            _fileService = fileService;
        }

        private static readonly string[] RegionalWeatherHeadings =
            {
                "The Weather at a Glance",
                "Regional Weather",
                "Regional Forecast",
                "Weather Outlook",
                "Across Sweden",
                "Sweden at a Glance",
                "Regional Conditions",
                "The National Forecast",
                "The Weather Picture",
                "Weather Across the Regions",
                "The Day's Weather",
                "The Weather Briefing",
                "A Snapshot of Sweden's Weather",
                "Weather from North to South"
            };


        [Route("Categories/{category?}")]
        public async Task<IActionResult> Index(string? category)
        {
            var selectedCategory = category ?? "Sweden";

            ViewBag.Category = selectedCategory.ToUpper();

            var articles = await _articleService
                .GetArticlesByCategoryAsync(selectedCategory);


            if (selectedCategory.Equals(
                "Weather", StringComparison.OrdinalIgnoreCase))
            {
                var municipality =
                    HttpContext.Session.GetString("SelectedWeatherMunicipality")
                    ?? "Gothenburg";

                var regionalForecasts = await Task.WhenAll(
                    _weatherService.Get24HourForecastAsync("Gothenburg"),
                    _weatherService.Get24HourForecastAsync("Stockholm"),
                    _weatherService.Get24HourForecastAsync("Umea")
                );
                var weatherArticle = await _context.WeatherArticles
                    .Where(a => a.City == "Sweden")
                    .OrderByDescending(a => a.GeneratedAt)
                    .FirstOrDefaultAsync(HttpContext.RequestAborted);

                if (weatherArticle != null)
                {
                    var dayNumber = DateOnly
                        .FromDateTime(weatherArticle.GeneratedAt)
                        .DayNumber;

                    var headingIndex =
                        dayNumber % RegionalWeatherHeadings.Length;

                    ViewBag.RegionalWeatherHeading =
                        RegionalWeatherHeadings[headingIndex];
                }
                else
                {
                    ViewBag.RegionalWeatherHeading =
                        RegionalWeatherHeadings[0];
                }


                ViewBag.WeatherArticle = weatherArticle;
                ViewBag.WeatherForecast = await _weatherService
                    .Get24HourForecastAsync(municipality);

                ViewBag.GotalandForecast = regionalForecasts[0];
                ViewBag.SvealandForecast = regionalForecasts[1];
                ViewBag.NorrlandForecast = regionalForecasts[2];

                return View("Weather", articles);
            }


            return View(articles);
        }
    }
}