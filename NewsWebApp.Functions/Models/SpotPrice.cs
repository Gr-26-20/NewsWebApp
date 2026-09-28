using System.Text.Json.Serialization;

namespace NewsWebApp.Functions.Models;

public class SpotPrice
{
    public int Hour { get; set; }

    [JsonPropertyName("price_eur")]
    public double PriceEur { get; set; }

    [JsonPropertyName("price_sek")]
    public double PriceSek { get; set; }

    public int Kmeans { get; set; }
}
