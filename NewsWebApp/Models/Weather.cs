namespace NewsWebApp.Models
{
    public class Weather
    {
        public int Id { get; set; }
        public string Summery { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }


    }
}
