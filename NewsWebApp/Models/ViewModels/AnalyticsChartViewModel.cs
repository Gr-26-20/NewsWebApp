namespace NewsWebApp.Models.ViewModels;

public class AnalyticsChartViewModel
{
    public string Title { get; set; } = "";
    public string Color { get; set; } = "#245c4f";
    public List<AnalyticsChartPoint> Points { get; set; } = [];
}

public record AnalyticsChartPoint(DateTime DateUtc, long? Value);
