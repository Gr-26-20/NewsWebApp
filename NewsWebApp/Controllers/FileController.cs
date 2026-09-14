using Microsoft.AspNetCore.Mvc;
using NewsWebApp.Models;
using NewsWebApp.Services;
using Stripe;

namespace NewsWebApp.Controllers
{
    public class FileController : Controller

    {

        private readonly IFileService _fileService;

        public FileController(IFileService fileService)

        {

            _fileService = fileService;

        }

        [HttpPost]

        public async Task<IActionResult> Upload(FileUploadModel model)

        {

            if (model.File == null || model.File.Length == 0)

            {

                return Content("File not selected");

            }

            await _fileService.UploadFileToContainer(model);
            return RedirectToAction("Index");

        }
    }
}