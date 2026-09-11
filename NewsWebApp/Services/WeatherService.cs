using System.Text.Json;
using NewsWebApp.Models.ViewModels;

namespace NewsWebApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        

        public async Task<WeatherStation?> GetWeatherAsync(string stationId)
        {
            var url = $"https://api.temperatur.nu/tnu_1.20.php?p={stationId}&sensor_type=air&cli=NewsWebApp";

            var response = await _httpClient.GetStringAsync(url);

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(response);

            return weather?.Stations.FirstOrDefault();
        }
        public async Task<List<WeatherStation>> GetStationsAsync(string municipality)
        {
            var url = $"https://api.temperatur.nu/tnu_1.20.php?kommun_url={municipality}&sensor_type=air&cli=NewsWebApp";

            var response = await _httpClient.GetStringAsync(url);

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(response);

            return weather?.Stations ?? new List<WeatherStation>();
        }
    }
}
