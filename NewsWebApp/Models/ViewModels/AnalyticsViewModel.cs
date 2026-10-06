namespace NewsWebApp.Models.ViewModels;

public class AnalyticsViewModel
{
    public int Days { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public int SuccessfulSubscriptions { get; set; }
    public decimal SubscriptionValue { get; set; }
    public long? PageViews { get; set; }
    public long? Visitors { get; set; }
    public long? FailedRequests { get; set; }
    public string? TelemetryMessage { get; set; }
    public List<PopularArticle> PopularArticles { get; set; } = [];
    public List<AnalyticsDay> Daily { get; set; } = [];
}
public class PopularArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Reads { get; set; }
}
public class AnalyticsDay
{
    public DateTime DateUtc { get; set; }
    public int Subscriptions { get; set; }
    public long? PageViews { get; set; }
    public long? FailedRequests { get; set; }
}
