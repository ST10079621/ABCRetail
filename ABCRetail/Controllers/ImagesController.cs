using ABCRetail.Services.AzureStorage;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ImagesController : Controller
    {
        private readonly BlobStorageService _blobStorageService;

        public ImagesController(
            BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var images =
                await _blobStorageService.GetBlobNamesAsync();

            return View(images);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please select an image.");

                return View();
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "",
                    "Only image files are allowed.");

                return View();
            }

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();

            await _blobStorageService.UploadAsync(
                stream,
                fileName,
                file.ContentType);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var result =
                await _blobStorageService.DownloadAsync(fileName);

            return File(
                result.Stream,
                result.ContentType,
                fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                await _blobStorageService.DeleteAsync(fileName);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
