using System.Text.Json;
using NewsWebApp.Models.ViewModels;
using System.Globalization;

namespace NewsWebApp.Services
{
    public class SmhiWeatherService
    {
        private readonly HttpClient _httpClient;

        public SmhiWeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SmhiWeatherData?> GetWeatherAsync(
            double latitude,
            double longitude)
        {
            var url =
                $"https://opendata-download-metfcst.smhi.se/api/category/snow1g/version/1/geotype/point/lon/{longitude.ToString(CultureInfo.InvariantCulture)}/lat/{latitude.ToString(CultureInfo.InvariantCulture)}/data.json";

            var response = await _httpClient.GetStringAsync(url);

            var weather =
    JsonSerializer.Deserialize<SmhiWeatherResponse>(response);

            var data = weather?.TimeSeries.FirstOrDefault()?.Data;

            if (data != null)
            {
                data.SymbolDescription = data.SymbolCode switch
                {
                    1 => "Clear sky",
                    2 => "Nearly clear sky",
                    3 => "Variable cloudiness",
                    4 => "Half clear sky",
                    5 => "Cloudy sky",
                    6 => "Overcast",
                    7 => "Fog",
                    8 => "Light rain showers",
                    9 => "Moderate rain showers",
                    10 => "Heavy rain showers",
                    11 => "Thunderstorm",
                    12 => "Light sleet showers",
                    13 => "Moderate sleet showers",
                    14 => "Heavy sleet showers",
                    15 => "Light snow showers",
                    16 => "Moderate snow showers",
                    17 => "Heavy snow showers",
                    18 => "Light rain",
                    19 => "Moderate rain",
                    20 => "Heavy rain",
                    21 => "Thunder",
                    22 => "Light sleet",
                    23 => "Moderate sleet",
                    24 => "Heavy sleet",
                    25 => "Light snowfall",
                    26 => "Moderate snowfall",
                    27 => "Heavy snowfall",
                    _ => "Unknown"
                };
            }

            return data;
        }
    }
}
