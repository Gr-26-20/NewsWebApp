namespace NewsWebApp.Models
{
    public class HistoricalData
    {
        public int Id { get; set; }
        public string ApiSource { get; set; } // e.g., "WeatherAPI", "CurrencyAPI"
        public string DataKey { get; set; } // e.g., "temperature", "exchange_rate"
        public string DataValue { get; set; } // JSON or serialized value
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string City { get; set; } // Optional, for weather data
    }
}
