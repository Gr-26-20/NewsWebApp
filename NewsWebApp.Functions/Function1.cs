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
    [TimerTrigger("0 0 * * * *")] TimerInfo timer)
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

                var stockholmTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

                var stockholmNow =
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        stockholmTimeZone);

                var localHour = stockholmNow.Hour;
             
                var currentSpotPrice =
                    await _tableStorageService.GetDayAheadPriceAsync(
                        stockholmNow.Date,
                        localHour);

                if (currentSpotPrice != null)
                {
                    _logger.LogInformation(
                        "SE3 Day-Ahead electricity price for hour {hour}: {price} öre/kWh",
                        localHour,
                        currentSpotPrice);

                    var measurementTime = DateTime.UtcNow;

                    var entity = new TemperatureElectricityEntity
                    {
                        PartitionKey = "Gothenburg",
                        RowKey = measurementTime.ToString("yyyy-MM-ddTHH"),
                        MeasurementTime = measurementTime,
                        TemperatureC = temperature,
                        ElectricityPrice = currentSpotPrice.Value
                    };

                    await _tableStorageService.SaveAsync(entity);

                    _logger.LogInformation(
                        "Saved temperature {temperature} °C and electricity price {price} öre/kWh to Azure Table at {time}",
                        temperature,
                        currentSpotPrice.Value,
                        measurementTime);
                }
            }
        }
    }

    [Function("SaveDayAheadElectricity")]
    public async Task SaveDayAheadElectricity(
    [TimerTrigger("0 0 14 * * *")] TimerInfo timer)
    {
        var stockholmTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        var stockholmNow =
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                stockholmTimeZone);

        var deliveryDate =
            stockholmNow.Date.AddDays(1);

        _logger.LogInformation(
            "Fetching Day-Ahead electricity prices for {date}",
            deliveryDate.ToString("yyyy-MM-dd"));

        var url =
            $"https://spotprices.lexlink.se/espot/{deliveryDate:yyyy-MM-dd}";

        var response =
            await _httpClient.GetStringAsync(url);

        var spotPrices =
            JsonSerializer.Deserialize<SpotPriceApiResponse>(
                response,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (spotPrices?.SE3 == null ||
            spotPrices.SE3.Count == 0)
        {
            _logger.LogWarning(
                "No SE3 Day-Ahead prices available for {date}",
                deliveryDate.ToString("yyyy-MM-dd"));

            return;
        }

        foreach (var price in spotPrices.SE3)
        {
            var entity = new DayAheadElectricityEntity
            {
                PartitionKey = "SE3",
                RowKey =
                    $"{deliveryDate:yyyy-MM-dd}T{price.Hour:00}",
                DeliveryDate = deliveryDate,
                Hour = price.Hour,
                ElectricityPrice = price.PriceSek
            };

            await _tableStorageService.SaveDayAheadAsync(entity);
        }

        _logger.LogInformation(
            "Saved {count} SE3 Day-Ahead prices for {date}",
            spotPrices.SE3.Count,
            deliveryDate.ToString("yyyy-MM-dd"));
    }
}