namespace NewsWebApp.Models
{
    public class WeatherForecast
    {
        public string Summary { get; set; }
        public string City { get; set; }
        public string Lang { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public long UnixTime { get; set; }
        public WeatherForecastIcon Icon { get; set; }
    }
}