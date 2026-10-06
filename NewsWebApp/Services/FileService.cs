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
    }
}