using Azure;
using Azure.Data.Tables;

namespace NewsWebApp.Models
{
    public class TemperatureElectricityEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public DateTime MeasurementTime { get; set; }

        public double TemperatureC { get; set; }
        public double ElectricityPrice { get; set; }
    }
}