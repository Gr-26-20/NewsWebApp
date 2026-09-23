using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace NewsWebApp.Functions;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly HttpClient _httpClient;

    public Function1(
        ILogger<Function1> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("SaveTemperatureElectricity")]
    public async Task Run(
        [TimerTrigger("*/30 * * * * *")] TimerInfo timer) // [TimerTrigger("0 0 * * * *")] change back to this

    {
        _logger.LogInformation(
            "Temperature/electricity collection ran at: {time}",
            DateTime.Now);

        var url =
            "https://api.temperatur.nu/tnu_1.20.php?p=angered_tv&sensor_type=air&cli=NewsWebApp";

        var response =
            await _httpClient.GetStringAsync(url);

        _logger.LogInformation(
            "Temperatur.nu response: {response}",
            response);
    }
}