using System.Text.Json.Serialization;

namespace NewsWebApp.Models.ViewModels
{
    public class SmhiWeatherResponse
    {
        [JsonPropertyName("timeSeries")]
        public List<SmhiTimeSeries> TimeSeries { get; set; } = new();
    }

    public class SmhiTimeSeries
    {
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("data")]
        public SmhiWeatherData Data { get; set; }
    }

    public class SmhiWeatherData
    {
        [JsonPropertyName("air_temperature")]
        public double AirTemperature { get; set; }

        [JsonPropertyName("symbol_code")]
        public int SymbolCode { get; set; }

        public string SymbolDescription { get; set; }
    }
}
