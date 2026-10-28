
using Azure.Core;
using Microsoft.Extensions.Configuration;
using NewsWebApp.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NewsWebApp.Services
{
    public class WeatherEditorialAiService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenCredential _credential;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        private readonly ConcurrentDictionary<string, SemaphoreSlim>
            _generationLocks = new();

        private static readonly TimeSpan MinimumGenerationInterval =
            TimeSpan.FromHours(1);

        private static readonly TimeSpan CacheLifetime =
            TimeSpan.FromHours(24);

        public WeatherEditorialAiService(
            HttpClient httpClient,
            TokenCredential credential,
            IConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _credential = credential;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }


        private sealed record CachedEditorial(
            string Editorial,
            string ForecastFingerprint,
            string WarningFingerprint,
            DateTimeOffset GeneratedAtUtc);


        public Task<string> GenerateEditorialAsync(
            string municipality,
            List<WeatherForecast> forecast,
            CancellationToken cancellationToken = default)
        {
            return GenerateEditorialAsync(
                municipality,
                forecast,
                null,
                cancellationToken);
        }


        public async Task<string> GenerateEditorialAsync(
            string municipality,
            List<WeatherForecast> forecast,
            List<SmhiWeatherWarning>? warnings,
            CancellationToken cancellationToken = default)
        {
            if (forecast == null || forecast.Count == 0)
            {
                throw new InvalidOperationException(
                    "No weather forecast data is available.");
            }

            var cacheKey =
                $"WeatherEditorial:{municipality.Trim().ToUpperInvariant()}";

            var forecastFingerprint = CreateForecastFingerprint(forecast);
            var warningFingerprint = CreateWarningFingerprint(warnings);

            var generationLock = _generationLocks.GetOrAdd(
                cacheKey,
                _ => new SemaphoreSlim(1, 1));

            await generationLock.WaitAsync(cancellationToken);

            try
            {
                _memoryCache.TryGetValue(
                    cacheKey,
                    out CachedEditorial? cached);

                if (cached != null)
                {
                    var forecastUnchanged =
                        cached.ForecastFingerprint == forecastFingerprint;

                    var warningsUnchanged =
                        cached.WarningFingerprint == warningFingerprint;

                    // Reuse the editorial when both data sets are unchanged.
                    if (forecastUnchanged && warningsUnchanged)
                    {
                        return cached.Editorial;
                    }

                    // Warning changes must trigger regeneration immediately.
                    // Forecast-only changes still respect the one-hour limit.
                    if (warningsUnchanged &&
                        DateTimeOffset.UtcNow - cached.GeneratedAtUtc
                            < MinimumGenerationInterval)
                    {
                        return cached.Editorial;
                    }
                }

                try
                {
                    var editorial = await GenerateEditorialUncachedAsync(
                        municipality,
                        forecast,
                        warnings,
                        cancellationToken);

                    var newCachedEditorial = new CachedEditorial(
                        editorial,
                        forecastFingerprint,
                        warningFingerprint,
                        DateTimeOffset.UtcNow);

                    _memoryCache.Set(
                        cacheKey,
                        newCachedEditorial,
                        CacheLifetime);

                    return editorial;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception) when (
                    cached != null &&
                    cached.WarningFingerprint == warningFingerprint)
                {
                    // Keep the previous editorial only when the warning data
                    // has not changed.
                    return cached.Editorial;
                }
            }
            finally
            {
                generationLock.Release();
            }
        }



        private static string CreateForecastFingerprint(
            List<WeatherForecast> forecast)
        {
            var snapshot = forecast
                .Select(item => new
                {
                    item.City,
                    item.Date,
                    item.Summary,
                    item.TemperatureC,
                    item.Humidity,
                    item.WindSpeed
                })
                .OrderBy(item => item.Date)
                .ToArray();

            var json = JsonSerializer.Serialize(snapshot);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));

            return Convert.ToHexString(hash);
        }


        private static string CreateWarningFingerprint(
            List<SmhiWeatherWarning>? warnings)
        {
            var snapshot = (warnings ?? new List<SmhiWeatherWarning>())
                .Select(warning => new
                {
                    warning.Id,
                    Event = warning.Event?.En,
                    Classification = warning.MhoClassification?.En,
                    Areas = warning.WarningAreas
                        .Select(area => new
                        {
                            area.Id,
                            AreaName = area.AreaName?.En,
                            WarningLevel = area.WarningLevel?.En,
                            EventDescription = area.EventDescription?.En,
                            area.ApproximateStart,
                            area.ApproximateEnd,
                            area.Published,
                            AffectedAreas = area.AffectedAreas
                                .Select(affectedArea => affectedArea.En)
                                .OrderBy(name => name)
                                .ToArray(),
                            Descriptions = area.Descriptions
                                .Select(description => new
                                {
                                    Title = description.Title?.En,
                                    Text = description.Text?.En
                                })
                                .ToArray()
                        })
                        .OrderBy(area => area.Id)
                        .ToArray()
                })
                .OrderBy(warning => warning.Id)
                .ToArray();

            var json = JsonSerializer.Serialize(snapshot);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));

            return Convert.ToHexString(hash);
        }



        private async Task<string> GenerateEditorialUncachedAsync(
            string municipality,
            List<WeatherForecast> forecast,
            List<SmhiWeatherWarning>? warnings,
            CancellationToken cancellationToken = default)
            {
            if (forecast == null || forecast.Count == 0)
            {
                throw new InvalidOperationException(
                    "No weather forecast data is available.");
            }

            var endpoint =
            _configuration["Foundry:AgentEndpoint"];

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException(
                    "The Foundry agent endpoint is not configured.");
            }

            endpoint += endpoint.Contains('?') ? "&" : "?";
            endpoint += "api-version=v1";

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException(
                    "The Foundry agent endpoint is not configured.");
            }

            var weatherData = forecast.Select(item => new
            {
                item.City,
                Date = item.Date.ToString(
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture),
                item.Summary,
                TemperatureC = $"{item.TemperatureC} °C",
                item.Humidity,
                WindSpeed = $"{item.WindSpeed} m/s"
            });

            var warningData = (warnings ?? new List<SmhiWeatherWarning>())
                .Select(warning => new
                {
                    warning.Id,
                    Event = warning.Event?.En,
                    Classification = warning.MhoClassification?.En,
                    Areas = warning.WarningAreas.Select(area => new
                    {
                        AreaName = area.AreaName?.En,
                        WarningLevel = area.WarningLevel?.En,
                        EventDescription = area.EventDescription?.En,
                        area.ApproximateStart,
                        area.ApproximateEnd,
                        AffectedAreas = area.AffectedAreas
                            .Select(affectedArea => affectedArea.En)
                            .ToList(),
                        Descriptions = area.Descriptions.Select(description => new
                        {
                            Title = description.Title?.En,
                            Text = description.Text?.En
                        }).ToList()
                    }).ToList()
                });

            var warningsJson = JsonSerializer.Serialize(
                warningData,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var forecastJson = JsonSerializer.Serialize(
                weatherData,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var input = $"""
                Write a weather editorial for CLICKBAIT News
                using the configured editorial instructions.

                Location: {municipality}

                Measurement units:
                TemperatureC is in degrees Celsius (°C).
                WindSpeed is in metres per second (m/s).
                Humidity is a percentage (%).

                Forecast data:
                {forecastJson}

                Official SMHI weather warnings:
                {warningsJson}

                Editorial requirements:
                - Base the forecast discussion on the supplied forecast data.
                - Use the SMHI warnings as additional official information.
                - Preserve warning severity, timing, and affected geographical areas.
                - Do not imply that a warning applies to a wider area than the data specifies.
                - Do not invent warnings or warning details.
                - If the warnings array is empty, do not invent any active warnings.
                - Integrate relevant warnings naturally into the editorial rather than
                  copying the warning data verbatim.
                """;

            var accessToken = await _credential.GetTokenAsync(
                new TokenRequestContext(
                    new[] { "https://ai.azure.com/.default" }),
                cancellationToken);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken.Token);

            request.Content = JsonContent.Create(new
            {
                input,
                stream = false
            });

            var requestBody = await request.Content.ReadAsStringAsync(
                cancellationToken);

            System.Diagnostics.Debug.WriteLine("FOUNDRY REQUEST BODY:");
            System.Diagnostics.Debug.WriteLine(requestBody);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);
            System.Diagnostics.Debug.WriteLine("FOUNDRY RAW RESPONSE:");
            System.Diagnostics.Debug.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Foundry returned HTTP {(int)response.StatusCode}: " +
                    responseBody);
            }
            Console.WriteLine(responseBody);

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("output_text", out var outputText))
            {
                return outputText.GetString()
                    ?? throw new InvalidOperationException(
                        "Foundry returned empty editorial text.");
            }

            if (root.TryGetProperty("output", out var output))
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var content))
                    {
                        continue;
                    }

                    foreach (var part in content.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var text)
                            && text.ValueKind == JsonValueKind.String)
                        {
                            return text.GetString()
                                ?? throw new InvalidOperationException(
                                    "Foundry returned empty editorial text.");
                        }
                    }
                }
            }

            throw new InvalidOperationException(
                "Foundry returned a response without editorial text.");
        }
    }
}

