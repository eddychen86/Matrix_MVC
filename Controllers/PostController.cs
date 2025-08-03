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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var article = new Article
                {
                    AuthorId = Guid.Parse("7ffbe594-de54-452a-92b0-311631587369"), // 假資料
                    Content = dto.Content,
                    IsPublic = dto.IsPublic,
                    Status = 0,
                    CreateTime = DateTime.Now,
                    PraiseCount = 0,
                    CollectCount = 0
                };

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                if (dto.Attachments != null)
                {
                    foreach (var file in dto.Attachments)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine("wwwroot/uploads", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
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
                    await _context.SaveChangesAsync();
                }

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

                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Create article failed");
                return StatusCode(500, "An error occurred while creating the article.");
            }
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
