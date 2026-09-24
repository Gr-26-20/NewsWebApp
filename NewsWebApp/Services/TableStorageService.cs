using Azure.Data.Tables;
using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureWebJobsStorage"];

            var tableName =
                "TemperatureElectricity";

            _tableClient = new TableClient(
                connectionString,
                tableName);
        }

        public async Task SaveAsync(
            TemperatureElectricityEntity entity)
        {
            await _tableClient.CreateIfNotExistsAsync();

            await _tableClient.UpsertEntityAsync(entity); // writes data to Azure
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