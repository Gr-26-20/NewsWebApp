using Azure;
using Azure.Data.Tables;

namespace NewsWebApp.Models
{
    public class DayAheadElectricityEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public DateTimeOffset DeliveryDate { get; set; }
        public int Hour { get; set; }
        public double ElectricityPrice { get; set; }
    }
}


