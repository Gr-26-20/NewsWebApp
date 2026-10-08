using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NewsWebApp.Data;
using NewsWebApp.Models;
using NewsWebApp.Models.ViewModels;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NewsWebApp.Services;

public class AnalyticsService(ApplicationDbContext db,
    IMemoryCache cache, // IMemoryCache is used for caching the analytics data
    IConfiguration configuration, // IConfiguration is used to access configuration settings
    IHttpClientFactory httpClients, // IHttpClientFactory is used to create HttpClient instances for making HTTP requests
    TokenCredential credential, // TokenCredential is used for authentication with Azure services
    ILogger<AnalyticsService> logger) // ILogger is used for logging
{
    public async Task<AnalyticsViewModel> GetDashboardAsync(int days, CancellationToken ct = default)
    {
        days = days is 7 or 30 or 90 ? days : 7;
        var key = $"analytics:{days}:{DateTime.UtcNow:yyyyMMdd}";
        if (cache.TryGetValue(key, out AnalyticsViewModel? cached) && cached != null) return cached;

        var now = DateTime.UtcNow;
        var from = now.Date.AddDays(-(days - 1));

        // pass filtered data to the view model
        var model = new AnalyticsViewModel { Days = days, FromUtc = from, UpdatedUtc = now };

        // SubscriptionAsync saves a record only after a successful payment.
        var subscriptions = await db.Subscriptions.AsNoTracking()
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= now)
            .GroupBy(s => s.CreatedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count(), Value = g.Sum(s => s.Price) }).ToListAsync(ct);
        model.SuccessfulSubscriptions = subscriptions.Sum(s => s.Count);
        model.SubscriptionValue = subscriptions.Sum(s => s.Value);
        model.PopularArticles = await db.Articles.AsNoTracking()
            .Where(a => a.articleStatus == Articles.Status.Approved && !a.IsArchived)
            .OrderByDescending(a => a.Views).ThenBy(a => a.Id).Take(5)
            .Select(a => new PopularArticle { Id = a.Id, Title = a.Title, Reads = a.Views }).ToListAsync(ct);
        model.Daily = Enumerable.Range(0, days).Select(i => new AnalyticsDay
        {
            DateUtc = from.AddDays(i),
            Subscriptions = subscriptions.FirstOrDefault(s => s.Date == from.AddDays(i))?.Count ?? 0
        }).ToList();
        var workspace = configuration["Analytics:WorkspaceId"];
        var resource = configuration["Analytics:ApplicationInsightsResourceId"];
        if (!Guid.TryParse(workspace, out _) || string.IsNullOrWhiteSpace(resource)
            || !resource.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
            model.TelemetryMessage = "Visitor and request analytics are not connected yet. Database statistics are available.";
        else
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(25));
                await LoadTelemetryAsync(model, workspace!, resource, timeout.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to load dashboard telemetry");
                model.TelemetryMessage = "Visitor and request analytics are temporarily unavailable. Database statistics are available.";
            }
        }
        cache.Set(key, model, TimeSpan.FromMinutes(5));
        return model;
    }

    private async Task LoadTelemetryAsync(AnalyticsViewModel model, string workspace, string resource, CancellationToken ct)
    {
        var scope = JsonSerializer.Serialize(resource);
        var from = model.FromUtc.ToString("O", CultureInfo.InvariantCulture);
        var to = model.UpdatedUtc.ToString("O", CultureInfo.InvariantCulture);

        // The query retrieves page views, unique visitors, and failed requests from Application Insights logs.
        var query = $$"""
            let pages = AppPageViews
                | where _ResourceId =~ {{scope}} and TimeGenerated between (datetime({{from}}) .. datetime({{to}}));
            let failures = AppRequests
                | where _ResourceId =~ {{scope}} and TimeGenerated between (datetime({{from}}) .. datetime({{to}}))
                | where Success == false;
            union
                (pages | summarize Value = sum(ItemCount) | extend Kind = 'PageViews', Day = datetime(null)),
                (pages | where isnotempty(UserId) | summarize Value = dcount(UserId) | extend Kind = 'Visitors', Day = datetime(null)),
                (failures | summarize Value = sum(ItemCount) | extend Kind = 'FailedRequests', Day = datetime(null)),
                (pages | summarize Value = sum(ItemCount) by Day = bin(TimeGenerated, 1d) | extend Kind = 'PageViewsDaily'),
                (failures | summarize Value = sum(ItemCount) by Day = bin(TimeGenerated, 1d) | extend Kind = 'FailuresDaily')
            | project Kind, Day, Value
            """;

        // Get an access token for the Log Analytics API using the provided TokenCredential.
        var token = await credential.GetTokenAsync(new TokenRequestContext(["https://api.loganalytics.io/.default"]), ct);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.loganalytics.azure.com/v1/workspaces/{workspace}/query");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Content = JsonContent.Create(new { query });

        // Send the request to the Log Analytics API and ensure a successful response.
        using var response = await httpClients.CreateClient("AnalyticsLogs").SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        // Parse the JSON response from the Log Analytics API and extract the relevant metrics.
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (json.RootElement.TryGetProperty("error", out _)) throw new InvalidOperationException("Incomplete analytics query.");

        // Extract the rows of data from the JSON response, which contain the metrics for page views, visitors, and failed requests.
        var rows = json.RootElement.GetProperty("tables")[0].GetProperty("rows");
        long views = 0, visitors = 0, failures = 0;
        var dailyViews = new Dictionary<DateTime, long>();
        var dailyFailures = new Dictionary<DateTime, long>();

        // Iterate through the rows returned by the analytics query and populate the corresponding metrics.
        foreach (var row in rows.EnumerateArray())
        {
            var value = row[2].ValueKind == JsonValueKind.Null ? 0 : (long)row[2].GetDouble();
            switch (row[0].GetString())
            {
                case "PageViews": views = value; break;
                case "Visitors": visitors = value; break;
                case "FailedRequests": failures = value; break;
                case "PageViewsDaily": dailyViews[row[1].GetDateTime().Date] = value; break;
                case "FailuresDaily": dailyFailures[row[1].GetDateTime().Date] = value; break;
            }
        }
        // Empty browser telemetry is unknown tracking rather than proven zero visits.
        model.PageViews = views > 0 ? views : null; // Set PageViews to null if there are no views, indicating unknown tracking.
        model.Visitors = visitors > 0 ? visitors : null;
        model.FailedRequests = failures;
        foreach (var day in model.Daily)
        {
            day.PageViews = views > 0 ? dailyViews.GetValueOrDefault(day.DateUtc) : null;
            day.FailedRequests = dailyFailures.GetValueOrDefault(day.DateUtc);
        }
        if (views == 0 || visitors == 0)
            model.TelemetryMessage = "No browser page views or visitor identifiers were found for this period. Check browser monitoring; missing visitor data is shown as unavailable.";
    }
}
