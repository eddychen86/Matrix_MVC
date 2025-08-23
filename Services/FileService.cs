
using Matrix.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Matrix.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string?> CreateFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                var webRoot = _webHostEnvironment.WebRootPath
                              ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

                // �T�O subfolder �O�۹���|
                subfolder = (subfolder ?? string.Empty).TrimStart('/', '\\');

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var uploadsFolder = Path.Combine(webRoot, subfolder.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // �^�ǥi������e�� src �Ϊ��۹���}
                var relativeUrl = "/" + Path.Combine(subfolder, uniqueFileName).Replace("\\", "/");
                _logger.LogInformation("File saved: {Physical} -> {Relative}", filePath, relativeUrl);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating file in subfolder {Subfolder}", subfolder);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            try
            {
                var webRoot = _webHostEnvironment.WebRootPath
                              ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(webRoot, relativePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return await Task.FromResult(true);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file: {Path}", relativePath);
                return false;
            }
        }


        public async Task<string?> UpdateFileAsync(IFormFile newFile, string? oldRelativePath, string subfolder)
        {
            if (!string.IsNullOrEmpty(oldRelativePath))
            {
                await DeleteFileAsync(oldRelativePath);
            }

            return await CreateFileAsync(newFile, subfolder);
        }
    }
}
