using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class LogsController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public LogsController(
            FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var files =
                await _fileStorageService.GetFileNamesAsync();

            return View(files);
        }

        public async Task<IActionResult> Download(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var stream =
                await _fileStorageService.DownloadAsync(
                    fileName);

            return File(
                stream,
                "text/plain",
                fileName);
        }
    }
}
