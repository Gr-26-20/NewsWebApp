using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface IFileService
    {
        Task UploadFileToContainer(FileUploadModel model);
    }
}
