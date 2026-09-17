namespace NewsWebApp.Models.ViewModels
{
    public class WeatherStationsResult
    {
        public List<WeatherStation> Stations { get; set; } = new();

        public bool IsBlocked { get; set; }

        public string ErrorMessage { get; set; }
    }
}
