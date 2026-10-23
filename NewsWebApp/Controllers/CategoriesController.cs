using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using NewsWebApp.Models;
using NewsWebApp.Services;
using NewsWebApp.Services;

namespace NewsWebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly WeatherService _weatherService;
        private readonly WeatherEditorialAiService _weatherEditorialAiService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IArticleService articleService,
            WeatherService weatherService,
            WeatherEditorialAiService weatherEditorialAiService,
            ILogger<CategoriesController> logger)
        {
            _articleService = articleService;
            _weatherService = weatherService;
            _weatherEditorialAiService = weatherEditorialAiService;
            _logger = logger;
        }

        [Route("Categories/{category?}")]
        public async Task<IActionResult> Index(string? category)
        {
            var selectedCategory = category ?? "Sweden";

            ViewBag.Category = selectedCategory.ToUpper();

            var articles = await _articleService
                .GetArticlesByCategoryAsync(selectedCategory);

            if (selectedCategory.Equals(
                "Weather",
                StringComparison.OrdinalIgnoreCase))
            {
                var municipality =
                    HttpContext.Session.GetString("SelectedWeatherMunicipality")
                    ?? "Gothenburg";

                var forecast =
                    await _weatherService.Get24HourForecastAsync(municipality);


                try
                {
                    var editorial =
                        await _weatherEditorialAiService.GenerateEditorialAsync(
                            municipality, forecast);

                    ViewBag.WeatherEditorial = editorial;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not generate weather editorial for {Municipality}.",
                        municipality);

                    // The Weather category can still display its regular articles.
                }

            }

            return View(articles);
        }
    }
}