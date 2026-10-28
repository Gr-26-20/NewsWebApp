using NewsWebApp.Models;
using System.Text.Json;

namespace NewsWebApp.Services
{
    public class SmhiWeatherWarningService
    {
        private const string WarningsEndpoint =
            "https://opendata-download-warnings.smhi.se/ibww/api/version/1/warning.json";

        private readonly HttpClient _httpClient;

        public SmhiWeatherWarningService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SmhiWeatherWarning>>
            GetActiveWarningsAsync(
                CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                WarningsEndpoint,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            var warnings =
                await JsonSerializer.DeserializeAsync<
                    List<SmhiWeatherWarning>>(
                    stream,
                    new JsonSerializerOptions(
                        JsonSerializerDefaults.Web),
                    cancellationToken);

            return warnings ?? new List<SmhiWeatherWarning>();
        }
    }
}