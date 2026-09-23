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

            await _tableClient.UpsertEntityAsync(entity);
        }
    }
}