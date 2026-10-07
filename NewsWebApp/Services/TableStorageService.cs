using Azure.Data.Tables;
using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;
        private readonly TableClient _dayAheadTableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureWebJobsStorage"];

            var tableName =
                "TemperatureElectricity";

            _tableClient = new TableClient(
                connectionString,
                tableName);

            _dayAheadTableClient = new TableClient(
                connectionString,
                "DayAheadElectricity");
        }

        public async Task SaveAsync(
            TemperatureElectricityEntity entity)
        {
            await _tableClient.CreateIfNotExistsAsync();

            await _tableClient.UpsertEntityAsync(entity);
        }

        public async Task SaveDayAheadAsync(
            DayAheadElectricityEntity entity)
        {
            await _dayAheadTableClient.CreateIfNotExistsAsync();

            await _dayAheadTableClient.UpsertEntityAsync(entity);
        }

        public async Task<double?> GetDayAheadPriceAsync(
            DateTime deliveryDate,
            int hour)
        {
            var rowKey =
                $"{deliveryDate:yyyy-MM-dd}T{hour:00}";

            try
            {
                var entity =
                    await _dayAheadTableClient.GetEntityAsync<DayAheadElectricityEntity>(
                        "SE3",
                        rowKey);

                return entity.Value.ElectricityPrice;
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<TemperatureElectricityEntity>> GetHistoryAsync() // Azure Table read method
        {
            await _tableClient.CreateIfNotExistsAsync();

            var results = new List<TemperatureElectricityEntity>();

            await foreach (var entity in _tableClient.QueryAsync<TemperatureElectricityEntity>(
                e => e.PartitionKey == "Gothenburg"))
            {
                results.Add(entity);
            }

            return results
                .OrderBy(e => e.MeasurementTime)
                .ToList();
        }
    }
}