using System.Text.Json.Serialization;

namespace NewsWebApp.Models.ViewModels
{
    public class WeatherApiResponse
    {
        [JsonPropertyName("client")]
        public string Client { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("stations")]
        public List<WeatherStation> Stations { get; set; } = new();
    }

    public class WeatherStation
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("temp")]
        public string Temp { get; set; }
    }
}
