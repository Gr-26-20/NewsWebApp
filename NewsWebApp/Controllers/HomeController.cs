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

        public HomeController(
            IArticleService articleService,
            WeatherService weatherService)
        {
            _articleService = articleService;
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index(string? stationId)
        {
            var model = new HomeViewModel
            {
                NewsArticles = (await _articleService
                    .GetArticlesByCategoryAsync("News"))
                    .Take(3)
                    .ToList(),

                WorldArticles = (await _articleService
                    .GetArticlesByCategoryAsync("World"))
                    .Take(1)
                    .ToList(),

                SwedenArticles = (await _articleService
                    .GetArticlesByCategoryAsync("Sweden"))
                    .Take(1)
                    .ToList(),

                SportsArticles = (await _articleService
                    .GetArticlesByCategoryAsync("Sports"))
                    .Take(1)
                    .ToList()
            };
            var selectedStationId = stationId ?? "angered_tv";

            var weather = await _weatherService.GetWeatherAsync(selectedStationId);

            model.Weather = weather;

            var stations = await _weatherService.GetStationsAsync("goteborg");

            model.WeatherStations = stations;

            return View(model);
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