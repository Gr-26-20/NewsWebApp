namespace NewsWebApp.Models.ViewModels
{
    public class TemperatureElectricityViewModel
    {
        public DateTime MeasurementTime { get; set; }

        public double TemperatureC { get; set; }

        public double ElectricityPrice { get; set; }
    }
}