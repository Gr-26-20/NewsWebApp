using Azure.Storage.Blobs; 
using NewsWebApp.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace NewsWebApp.Services
{
    public class FileService : IFileService
    {
        
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)

        {

            _configuration = configuration;

        }

        

        public async Task<string> UploadFileToContainer(FileUploadModel model)
        {
            return await UploadImageAsync(model.File);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            string connectionString = _configuration["AzureWebJobsStorage"];

            string containerName = _configuration["BlobContainerName"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient =
                                                blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it does not exist
            await containerClient.CreateIfNotExistsAsync(publicAccessType: Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            // Use a unique blob name so uploads never overwrite each other
            string blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            using (var stream = file.OpenReadStream())
            {
                Console.WriteLine("===== STARTING BLOB UPLOAD =====");

                await blobClient.UploadAsync(stream, blobHttpHeaders);

                Console.WriteLine("===== BLOB UPLOAD FINISHED =====");
            }

            Console.WriteLine("===== RETURNING BLOB URL =====");

            return blobClient.Uri.ToString();
        }


        public async Task<string> UploadWeatherImageAsync(
            byte[] imageBytes,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            string connectionString =
                _configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");

            string containerName =
                _configuration["WeatherImageBlobContainer"]
                ?? throw new InvalidOperationException(
                    "WeatherImageBlobContainer is not configured.");

            var blobServiceClient = new BlobServiceClient(connectionString);

            var containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(
                cancellationToken: cancellationToken);

            string blobName =
                $"weather/{date:yyyy-MM-dd}-{Guid.NewGuid():N}.png";

            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(imageBytes);

            await blobClient.UploadAsync(
                stream,
                overwrite: true,
                cancellationToken: cancellationToken);

            await blobClient.SetHttpHeadersAsync(
                new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = "image/png",
                    CacheControl = "public, max-age=3600"
                },
                cancellationToken: cancellationToken);

            return blobClient.Uri.ToString();
        }

    }
}