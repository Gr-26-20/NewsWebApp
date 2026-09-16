using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using NewsWebApp.Models.ViewModels;


namespace NewsWebApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }



        public async Task<WeatherStation?> GetWeatherAsync(string stationId)
        {
            var cacheKey = $"weather_{stationId}";

            if (_cache.TryGetValue(cacheKey, out WeatherStation? cachedWeather))
            {
                return cachedWeather;
            }

            var url = $"https://api.temperatur.nu/tnu_1.20.php?p={stationId}&sensor_type=air&cli=NewsWebApp";

            var response = await _httpClient.GetStringAsync(url);

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(response);

            var station = weather?.Stations.FirstOrDefault();

            if (station != null)
            {
                _cache.Set(
                    cacheKey,
                    station,
                    TimeSpan.FromMinutes(5));
            }

            return station;
        }
        public async Task<WeatherStationsResult> GetStationsAsync(string municipality)
        {
            var cacheKey = $"stations_{municipality}";

            if (_cache.TryGetValue(cacheKey, out List<WeatherStation>? cachedStations))
            {
                return new WeatherStationsResult
                {
                    Stations = cachedStations
                };
            }

            var url =
                $"https://api.temperatur.nu/tnu_1.20.php?kommun_url={municipality}&sensor_type=air&cli=NewsWebApp";

            var response =
                await _httpClient.GetStringAsync(url);

            var weather =
                JsonSerializer.Deserialize<WeatherApiResponse>(response);

            if (weather?.Client == "blocked")
            {
                return new WeatherStationsResult
                {
                    IsBlocked = true,
                    ErrorMessage = weather.Error
                };
            }

            var stations =
                weather?.Stations ?? new List<WeatherStation>();

            if (stations.Count > 0)
            {
                _cache.Set(
                    cacheKey,
                    stations,
                    TimeSpan.FromMinutes(5));
            }

            return new WeatherStationsResult
            {
                Stations = stations
            };
        }
    }
}
