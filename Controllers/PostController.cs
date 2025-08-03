using Microsoft.AspNetCore.Mvc;
using Matrix.DTOs;
using Matrix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Matrix.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostController> _logger;
        public PostController(ApplicationDbContext context, ILogger<PostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Create([FromForm] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 建立文章
            var article = new Article
            {
                AuthorId = Guid.Parse("870c0b75-97a3-4e4f-8215-204d5747d28c"), // 假資料
                Content = dto.Content,
                IsPublic = dto.IsPublic,
                Status = 0,
                CreateTime = DateTime.Now,
                PraiseCount = 0,
                CollectCount = 0
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync(); // 先存文章，確保 ArticleId 已產生

            // 附加檔案處理
            if (dto.Attachments != null)
            {
                foreach (var file in dto.Attachments)
                {
                    var fileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // 確保目錄存在
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var attachment = new ArticleAttachment
                    {
                        FileId = Guid.NewGuid(),
                        ArticleId = article.ArticleId,
                        FilePath = "/uploads/" + fileName,
                        Type = file.ContentType?.StartsWith("image/") == true ? "image" : "file",
                        FileName = file.FileName,
                        MimeType = file.ContentType
                    };
                    _context.ArticleAttachments.Add(attachment);
                }
                await _context.SaveChangesAsync(); // 建議附件也立即存
            }
            // Hashtag 關聯
            if (dto.SelectedHashtags != null && dto.SelectedHashtags.Any())
            {
                foreach (var tagId in dto.SelectedHashtags)
                {
                    var articleHashtag = new ArticleHashtag
                    {
                        ArticleId = article.ArticleId,
                        TagId = tagId
                    };
                    _context.ArticleHashtags.Add(articleHashtag);
                }
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetHashtags()
        {
            var hashtags = await _context.Hashtags.ToListAsync();
            var data = hashtags.Select(x => new {
                x.TagId,
                x.Content
            });
            return Json(data);
        }
    }
}
