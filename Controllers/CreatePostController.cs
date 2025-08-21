using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Matrix.DTOs;
using Matrix.Models;    
using Matrix.Services.Interfaces;     
using Matrix.Repository.Interfaces;      

namespace Matrix.Controllers
{
    [Route("[controller]")]
    public class CreatePostController(
        IArticleService articleService,
        IHashtagRepository hashtagRepository,
        IFileService fileService,
        IArticleAttachmentRepository articleAttachmentRepository,
        ILogger<CreatePostController> logger
    ) : Controller
    {
        private readonly IArticleService _articleService = articleService;
        private readonly IHashtagRepository _hashtagRepository = hashtagRepository;
        private readonly IFileService _fileService = fileService;
        private readonly IArticleAttachmentRepository _articleAttachmentRepository = articleAttachmentRepository;
        private readonly ILogger<CreatePostController> _logger = logger;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = HttpContext.Items["UserId"] as Guid?;
            if (!currentUserId.HasValue)
                return Unauthorized(new { message = "請先登入後再發文" });

            try
            {
                var result = await _articleService.CreateArticleWithAttachmentsAsync(currentUserId.Value, dto);
                if (result == null)
                    return BadRequest(new { message = "建立文章失敗" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create article failed");
                return StatusCode(500, "建立文章過程中發生錯誤");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] List<IFormFile> files,
            [FromForm] Guid articleId,
            [FromForm] string mode
        )
        {
            const long MaxSize = 5 * 1024 * 1024;
            const int MaxCount = 6;

            if (files == null || files.Count == 0)
                return BadRequest(new { success = false, message = "請選擇檔案" });

            mode = (mode ?? "").Trim().ToLowerInvariant();
            _logger.LogInformation("UploadAttachment: mode={Mode}, count={Count}", mode, files.Count);

            if (mode != "image" && mode != "file")
                return BadRequest(new { success = false, message = "mode 必須是 image 或 file" });

            static bool IsImageExt(string fileName)
            {
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
            }

            static bool IsImageByContentType(string? contentType)
                => !string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

            static bool IsImage(IFormFile f)
                => IsImageByContentType(f.ContentType) || IsImageExt(f.FileName);

            var distinct = files
                .Where(f => f?.Length > 0)
                .GroupBy(f => f.FileName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (distinct.Count > MaxCount)
                return BadRequest(new { success = false, message = $"{(mode == "image" ? "圖片" : "檔案")}最多 {MaxCount}" });

            foreach (var f in distinct)
            {
                if (f.Length > MaxSize)
                    return BadRequest(new { success = false, message = $"檔案 {f.FileName} 超過 5MB" });

                var actuallyImage = IsImage(f);

                if (mode == "image" && !actuallyImage)
                    return BadRequest(new { success = false, message = $"檔案 {f.FileName} 不是圖片" });

                if (mode == "file" && actuallyImage)
                    return BadRequest(new { success = false, message = $"檔案 {f.FileName} 為圖片，請用圖片上傳區" });
            }

            var subfolder = mode == "image" ? "public/posts/imgs" : "public/posts/files";

            var saved = new List<string>();
            try
            {
                foreach (var f in distinct)
                {
                    var path = await _fileService.CreateFileAsync(f, subfolder);
                    if (string.IsNullOrEmpty(path))
                        throw new InvalidOperationException($"檔案 {f.FileName} 儲存失敗");

                    await _articleAttachmentRepository.AddAsync(new ArticleAttachment
                    {
                        FileId = Guid.NewGuid(),
                        ArticleId = articleId,
                        FilePath = path
                    });

                    _logger.LogInformation("Saved attachment: {Path}", path);
                    saved.Add(path);
                }

                return Ok(new { success = true, message = "上傳成功", data = saved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "附件上傳失敗");
                foreach (var p in saved) { try { await _fileService.DeleteFileAsync(p); } catch { } }
                return StatusCode(500, new { success = false, message = "上傳過程中發生錯誤" });
            }
        }


        [HttpGet("GetHashtags")]
        public async Task<IActionResult> GetHashtags()
        {
            var hashtags = await _hashtagRepository.GetAllAsync();
            var data = hashtags.Select(x => new { x.TagId, x.Content });
            return Json(data);
        }
    }
}
