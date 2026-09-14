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

        public async Task UploadFileToContainer(FileUploadModel model)

        {

            string connectionString = _configuration.GetConnectionString("AzureWebJobsStorage");

            string containerName = _configuration.GetConnectionString("BlobContainerName");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient =

                                                blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it does not exist 

            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(model.File.FileName);

            using (var stream = model.File.OpenReadStream())

            {

                await blobClient.UploadAsync(stream, true);

            }

        }
    }
}