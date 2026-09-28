namespace NewsWebApp.Functions.Models;

public class TemperatureApiResponse
{
    public string Client { get; set; }

    public List<TemperatureStation> Stations { get; set; } = new();
}
