using Microsoft.AspNetCore.Http;
using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface IFileService
    {
        Task<string> UploadFileToContainer(FileUploadModel model);
        Task<string> UploadImageAsync(IFormFile file);
    }
}
