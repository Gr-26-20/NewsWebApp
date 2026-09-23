using NewsWebApp.Functions.Models;
using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using NewsWebApp.Services;
using NewsWebApp.Models;

namespace NewsWebApp.Functions;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly HttpClient _httpClient;
    private readonly TableStorageService _tableStorageService;

    public Function1(
        ILogger<Function1> logger,
        IHttpClientFactory httpClientFactory,
        TableStorageService tableStorageService)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _tableStorageService = tableStorageService;
    }

    [Function("SaveTemperatureElectricity")]
    public async Task Run(
        [TimerTrigger("*/30 * * * * *")] TimerInfo timer)
    {
        _logger.LogInformation(
            "Temperature/electricity collection ran at: {time}",
            DateTime.Now);

        var url =
            "https://api.temperatur.nu/tnu_1.20.php?p=angered_tv&sensor_type=air&cli=NewsWebApp";

        var response =
            await _httpClient.GetStringAsync(url);

        var weather =
            JsonSerializer.Deserialize<TemperatureApiResponse>(
                response,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        var station = weather?.Stations.FirstOrDefault();

        _logger.LogInformation(
            "Number of stations received: {count}",
            weather?.Stations.Count ?? 0);

        if (station != null)
        {
            if (double.TryParse(
                station.Temp,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double temperature))
            {
                _logger.LogInformation(
                    "Station: {station}, Temperature: {temperature} °C",
                    station.Title,
                    temperature);

                var measurementTime = DateTime.UtcNow; 

                var entity = new TemperatureElectricityEntity
                {
                    PartitionKey = "Gothenburg",
                    RowKey = measurementTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    MeasurementTime = measurementTime,
                    TemperatureC = temperature,
                    ElectricityPrice = 0
                };

                await _tableStorageService.SaveAsync(entity);

                _logger.LogInformation(
                    "Saved temperature {temperature} °C to Azure Table at {time}",
                    temperature,
                    measurementTime);
            }
        }
    }
}