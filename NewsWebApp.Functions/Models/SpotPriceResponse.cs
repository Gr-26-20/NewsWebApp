namespace NewsWebApp.Functions.Models;

public class SpotPriceApiResponse
{
    public string Date { get; set; }

    public List<SpotPrice> SE1 { get; set; } = new();

    public List<SpotPrice> SE2 { get; set; } = new();

    public List<SpotPrice> SE3 { get; set; } = new();

    public List<SpotPrice> SE4 { get; set; } = new();
}