using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using NewsWebApp.Services;
using System.Diagnostics;

namespace NewsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly WeatherService _weatherService;
        private readonly SmhiWeatherService _smhiWeatherService;

        public HomeController(
            IArticleService articleService,
            WeatherService weatherService,
            SmhiWeatherService smhiWeatherService)
        {
            _articleService = articleService;
            _weatherService = weatherService;
            _smhiWeatherService = smhiWeatherService;
        }

        public async Task<IActionResult> Index(string? stationId)
        {
            var model = new HomeViewModel
            {
                NewsArticles = (await _articleService
                    .GetApprovedArticlesByCategoryAsync("News"))
                    .Take(3)
                    .ToList(),

                WorldArticles = (await _articleService
                    .GetApprovedArticlesByCategoryAsync("World"))
                    .Take(1)
                    .ToList(),

                SwedenArticles = (await _articleService
                    .GetArticlesByCategoryAsync("Sweden"))
                    .Take(1)
                    .ToList(),

                SportsArticles = (await _articleService
                    .GetApprovedArticlesByCategoryAsync("Sports"))
                    .Take(1)
                    .ToList(),

                EditorsChoiceArticles = (await _articleService
                    .GetEditorsChoiceArticlesAsync())
                    .Take(3)
                    .ToList()
            };
            var selectedStationId = stationId ?? "angered_tv";

            var weather = await _weatherService.GetWeatherAsync(selectedStationId);

            model.Weather = weather;

            var stationsResult = await _weatherService.GetStationsAsync("goteborg");

            model.WeatherStations = stationsResult.Stations;

            var gothenburg = model.Municipalities
                .First(m => m.ApiValue == "goteborg");

            var smhiWeather = await _smhiWeatherService.GetWeatherAsync(
                gothenburg.Latitude,
                gothenburg.Longitude);

            model.SmhiWeather = smhiWeather;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetWeather(string stationId)
        {
            var weather = await _weatherService.GetWeatherAsync(stationId);

            if (weather == null)
            {
                return NotFound();
            }

            return Json(weather);
        }

        [HttpGet]
        public async Task<IActionResult> GetStations(string municipality)
        {
            var result = await _weatherService.GetStationsAsync(municipality);

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSmhiWeather(string municipality)
        {
            var model = new HomeViewModel();

            var selectedMunicipality = model.Municipalities
                .FirstOrDefault(m => m.ApiValue == municipality);

            if (selectedMunicipality == null)
            {
                return NotFound();
            }

            var weather = await _smhiWeatherService.GetWeatherAsync(
                selectedMunicipality.Latitude,
                selectedMunicipality.Longitude);

            if (weather == null)
            {
                return NotFound();
            }

            return Json(weather);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}