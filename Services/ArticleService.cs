using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.Models;
using Matrix.DTOs;
using Matrix.Services.Interfaces;
using Matrix.Repository.Interfaces;
using AutoMapper;

namespace Matrix.Services
{
    /// <summary>
    /// 文章服務
    /// </summary>
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IArticleAttachmentRepository _attachmentRepository;
        private readonly IMapper _mapper;

        public ArticleService(ApplicationDbContext context, IFileService fileService, IArticleAttachmentRepository attachmentRepository, IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _attachmentRepository = attachmentRepository;
            _mapper = mapper;
        }
        /// <summary>
        /// 根據ID獲取文章 - 優化版本
        /// </summary>
        public async Task<ArticleDto?> GetArticleAsync(Guid id)
        {
            var article = await _context.Articles
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null) return null;

            // 如果需要回覆資料，單獨查詢避免 N+1 問題
            var replies = await _context.Replies
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ArticleId == id)
                .ToListAsync();

            article.Replies = replies;
            return _mapper.Map<ArticleDto>(article);
        }

        /// <summary>
        /// 建立文章
        /// </summary>
        public async Task<bool> CreateArticleAsync(CreateArticleDto dto, Guid authorId)
        {
            var author = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == authorId);
            if (author == null) return false;

            _context.Articles.Add(new Article
            {
                AuthorId = author.PersonId,
                Content = dto.Content,
                IsPublic = dto.IsPublic,
                Status = 0,
                CreateTime = DateTime.Now,
                PraiseCount = 0,
                CollectCount = 0
            });

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新文章
        /// </summary>
        public async Task<bool> UpdateArticleAsync(Guid id, string content, int isPublic, Guid authorId)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article?.Author?.UserId != authorId) return false;

            article.Content = content;
            article.IsPublic = isPublic;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 刪除文章
        /// </summary>
        public async Task<bool> DeleteArticleAsync(Guid id, Guid authorId)
        {
            return await UpdateArticleFieldAsync(id, authorId, a => a.Status = 2);
        }

        /// <summary>
        /// 獲取文章列表
        /// </summary>
        public async Task<(List<ArticleDto> Articles, int TotalCount)> GetArticlesAsync(
            int page = 1, int pageSize = 20, string? searchKeyword = null, Guid? authorId = null
        )
        {
            var query = BuildArticleQuery(searchKeyword, authorId);
            var totalCount = await query.CountAsync();

            var articles = await query
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articleDtos = _mapper.Map<List<ArticleDto>>(articles);

            return (articleDtos, totalCount);
        }

        /// <summary>
        /// 增加讚數
        /// </summary>
        public async Task<bool> IncreasePraiseCountAsync(Guid articleId)
        {
            return await UpdateCountAsync(articleId, a => a.PraiseCount++);
        }

        /// <summary>
        /// 減少讚數
        /// </summary>
        public async Task<bool> DecreasePraiseCountAsync(Guid articleId)
        {
            return await UpdateCountAsync(articleId, a => { if (a.PraiseCount > 0) a.PraiseCount--; });
        }

        /// <summary>
        /// 增加收藏數
        /// </summary>
        public async Task<bool> IncreaseCollectCountAsync(Guid articleId)
        {
            return await UpdateCountAsync(articleId, a => a.CollectCount++);
        }

        /// <summary>
        /// 減少收藏數
        /// </summary>
        public async Task<bool> DecreaseCollectCountAsync(Guid articleId)
        {
            return await UpdateCountAsync(articleId, a => { if (a.CollectCount > 0) a.CollectCount--; });
        }

        /// <summary>
        /// 新增回覆
        /// </summary>
        public async Task<bool> CreateReplyAsync(CreateReplyDto dto, Guid authorId)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == dto.ArticleId);
            if (article == null) return false;

            _context.Replies.Add(new Reply
            {
                ArticleId = dto.ArticleId,
                UserId = authorId,
                Content = dto.Content,
                ReplyTime = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新文章狀態
        /// </summary>
        public async Task<bool> UpdateArticleStatusAsync(Guid id, int status)
        {
            return await UpdateCountAsync(id, a => a.Status = status);
        }

        /// <summary>
        /// 獲取熱門文章
        /// </summary>
        public async Task<List<ArticleDto>> GetPopularArticlesAsync(int limit = 10, int days = 7)
        {
            var sinceDate = DateTime.Now.AddDays(-days);

            var articles = await _context.Articles
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Where(a => a.Status == 0 && a.IsPublic == 0 && a.CreateTime >= sinceDate)
                .OrderByDescending(a => a.PraiseCount + a.CollectCount)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<ArticleDto>>(articles);
        }

        /// <summary>
        /// 搜尋文章
        /// </summary>
        public async Task<(List<ArticleDto> Items, int TotalCount)> SearchAsync(
            string? keyword = null, int page = 1, int pageSize = 20)
        {
            var (articles, totalCount) = await GetArticlesAsync(page, pageSize, keyword);
            return (articles, totalCount);
        }

        /// <summary>
        /// 更新狀態
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            return await UpdateArticleStatusAsync(id, status);
        }

        // 私有輔助方法 - 優化版本，使用 Select 投影避免 N+1 查詢
        private IQueryable<Article> BuildArticleQuery(string? searchKeyword, Guid? authorId)
        {
            var query = _context.Articles
                .AsNoTracking() // 只讀查詢
                // 使用 Select 投影只加載需要的欄位，提升性能
                .Where(a => a.Status == 0 && a.IsPublic == 0);

            if (authorId.HasValue)
                query = query.Where(a => a.AuthorId == authorId.Value);

            if (!string.IsNullOrEmpty(searchKeyword))
                query = query.Where(a => a.Content.Contains(searchKeyword));

            // 使用單一 Include 避免複雜的關聯
            return query.Include(a => a.Author);
        }

        private async Task<bool> UpdateCountAsync(Guid articleId, Action<Article> updateAction)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            if (article == null) return false;

            updateAction(article);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> UpdateArticleFieldAsync(Guid id, Guid authorId, Action<Article> updateAction)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article?.Author?.UserId != authorId) return false;

            updateAction(article);
            await _context.SaveChangesAsync();
            return true;
        }

        // 映射方法已由 AutoMapper 取代

        public async Task<ArticleDto?> CreateArticleWithAttachmentsAsync(Guid authorId, CreateArticleDto dto)
        {
            var author = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == authorId);
            if (author == null) return null;

            var article = new Article
            {
                ArticleId = Guid.NewGuid(),
                AuthorId = author.PersonId,
                Content = dto.Content,
                IsPublic = dto.IsPublic,
                CreateTime = DateTime.UtcNow,
                Status = 0
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var file in dto.Attachments)
                {
                    var filePath = await _fileService.CreateFileAsync(file, "posts/files");
                    if (filePath != null)
                    {
                        var attachment = new ArticleAttachment
                        {
                            FileId = Guid.NewGuid(),
                            ArticleId = article.ArticleId,
                            FilePath = filePath,
                            FileName = file.FileName,
                            Type = file.ContentType.StartsWith("image") ? "image" : "file",
                            MimeType = file.ContentType
                        };
                        await _attachmentRepository.AddAsync(attachment);
                    }
                }
            }

            return await GetArticleAsync(article.ArticleId);
        }
    }
}