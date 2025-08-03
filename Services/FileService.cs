
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
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "public", subfolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/public/{subfolder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating file in subfolder {Subfolder}", subfolder);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;

            try
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
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
