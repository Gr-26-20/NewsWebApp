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



        public ForecastController(
        WeatherService weatherService,
        TableStorageService tableStorageService,
        WeatherEditorialAiService weatherEditorialAiService,
        WeatherImageAiService weatherImageAiService,
        IFileService fileService,
        ApplicationDbContext context,
        SmhiWeatherWarningService smhiWeatherWarningService)
        {
            _weatherService = weatherService;
            _tableStorageService = tableStorageService;
            _weatherEditorialAiService = weatherEditorialAiService;
            _weatherImageAiService = weatherImageAiService;
            _smhiWeatherWarningService = smhiWeatherWarningService;
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
                    // Get forecasts representing the three main regions of Sweden.
                    var regionalForecasts = await Task.WhenAll(
                        _weatherService.Get24HourForecastAsync("Gothenburg"),
                        _weatherService.Get24HourForecastAsync("Stockholm"),
                        _weatherService.Get24HourForecastAsync("Umea")
                    );

                    var forecast = regionalForecasts
                        .SelectMany(region => region)
                        .ToList();

                    // Get active official SMHI warnings.
                    var warnings =
                        await _smhiWeatherWarningService.GetActiveWarningsAsync();

                    // Generate the national editorial and image brief.
                    var editorial = await _weatherEditorialAiService
                        .GenerateEditorialAsync(
                            "Sweden",
                            forecast,
                            warnings,
                            cancellationToken);

                    // Extract IMAGE BRIEF from the editorial.
                    var imageBriefMarker = "IMAGE BRIEF:";

                    var imageBriefIndex = editorial.IndexOf(
                        imageBriefMarker,
                        StringComparison.OrdinalIgnoreCase);

                    if (imageBriefIndex < 0)
                    {
                        return Problem(
                            "The generated editorial did not contain an IMAGE BRIEF section.");
                    }

                    var imageBrief = editorial[
                        (imageBriefIndex + imageBriefMarker.Length)..].Trim();

                    if (string.IsNullOrWhiteSpace(imageBrief))
                    {
                        return Problem(
                            "The generated editorial contained an empty IMAGE BRIEF section.");
                    }

                    // Build the image-generation prompt from the selected weather story.
                    var prompt = $"""
                        Create one realistic editorial weather photograph for a Swedish
                        national news website, illustrating the following weather story.

                        IMAGE BRIEF:
                        {imageBrief}

                        Follow the geographical setting and weather conditions described
                        in the image brief. Depict only conditions supported by the brief.
                        Do not automatically use Gothenburg or any other city unless the
                        brief calls for it.

                        Show ONE single, continuous photographic scene captured from
                        ONE camera viewpoint. The landscape must form one coherent,
                        uninterrupted environment from the left edge to the right edge.

                        ABSOLUTELY NO collages, triptychs, split screens, diptychs,
                        multiple panels, side-by-side photographs, tiled scenes,
                        composite layouts, or visible divisions between scenes.

                        Do not attempt to illustrate several Swedish regions at once.
                        Focus on the single most newsworthy weather event described
                        in the image brief.

                        Use natural colours, realistic photographic lighting and a
                        believable Swedish landscape appropriate to the weather event.

                        Compose a wide panoramic photograph intended for a final aspect
                        ratio of 1500x417 pixels. Keep the horizon and important visual
                        details within the central horizontal band. Extend the same
                        continuous scenery naturally to both edges.

                        This must look like one genuine landscape photograph, not a
                        collection of images or an infographic.

                        Do not include text, logos, watermarks or graphic overlays.
                        """;

                    var imageBytes = await _weatherImageAiService
                        .GenerateImageAsync(prompt, cancellationToken);

                    Console.WriteLine(
                        $"Generated weather image: {imageBytes.Length} bytes");

                    // Keep your existing image upload and database-saving code here.

                    var imageUrl = await _fileService.UploadWeatherImageAsync(
                        imageBytes,
                        DateOnly.FromDateTime(DateTime.Now),
                        cancellationToken);

                    // Save the editorial and image URL in SQL.
                    var article = new WeatherArticle
                    {
                        City = "Sweden",
                        ForecastDate = DateTime.Today,
                        EditorialText = editorial,
                        ImageUrl = imageUrl,
                        GeneratedAt = DateTime.UtcNow
                    };

                    _context.WeatherArticles.Add(article);
                    await _context.SaveChangesAsync(cancellationToken);

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
