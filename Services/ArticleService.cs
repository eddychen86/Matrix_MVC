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
        private readonly IArticleHashtagRepository _articleHashtagRepository;
        private readonly IMapper _mapper;

        public ArticleService(ApplicationDbContext context, IFileService fileService, IArticleAttachmentRepository attachmentRepository, IArticleHashtagRepository articleHashtagRepository, IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _attachmentRepository = attachmentRepository;
            _articleHashtagRepository = articleHashtagRepository;
            _mapper = mapper;
        }
        /// <summary>
        /// 根據ID獲取文章 - 優化版本
        /// </summary>
        public async Task<ArticleDto?> GetArticleAsync(Guid id)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Attachments!)
                .Include(a => a.ArticleHashtags!)
                    .ThenInclude(ah => ah.Hashtag)
                .Include(a => a.Replies!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null) return null;
            return _mapper.Map<ArticleDto>(article);
        }


        ///<summary>
        ///取得單篇文章詳情（含作者、附件、hashtags）
        /// </summary>
        public async Task<ArticleDto?> GetArticleDetailAsync(Guid articleId)
        {
            return await _context.Set<Article>()
                .AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    AuthorId = a.AuthorId,
                    Content = a.Content,
                    IsPublic = a.IsPublic,
                    Status = a.Status,
                    CreateTime = a.CreateTime,
                    PraiseCount = a.PraiseCount,
                    CollectCount = a.CollectCount,

                    Author = a.Author == null ? null : new PersonDto
                    {
                        PersonId = a.Author.PersonId,
                        UserId = a.Author.UserId,
                        DisplayName = a.Author.DisplayName,
                        AvatarPath = a.Author.AvatarPath
                    },

                    Attachments = a.Attachments!.Select(att => new ArticleAttachmentDto
                    {
                        FileId = att.FileId,
                        FilePath = att.FilePath,
                        FileName = att.FileName,
                        Type = att.Type
                    }).ToList(),

                    Hashtags = a.ArticleHashtags!
                        .Where(ah => ah.Hashtag != null)
                        .Select(ah => new HashtagDto
                        {
                            TagId = ah.TagId,
                            Name = ah.Hashtag!.Content
                        }).ToList()
                })
                .FirstOrDefaultAsync();
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
            int page = 1, int pageSize = 20, string? searchKeyword = null, Guid? authorId = null,
            int? status = null, bool onlyPublic = true, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                Console.WriteLine($"GetArticlesAsync called - Page: {page}, PageSize: {pageSize}, AuthorId: {authorId}");

                var query = BuildArticleQuery(searchKeyword, authorId, status, onlyPublic, dateFrom, dateTo);
                Console.WriteLine($"Query built successfully");

                var totalCount = await query.CountAsync();
                Console.WriteLine($"Total count: {totalCount}");

                var articles = await query
                    .OrderByDescending(a => a.CreateTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Console.WriteLine($"Articles fetched: {articles.Count}");

                var articleDtos = _mapper.Map<List<ArticleDto>>(articles);
                Console.WriteLine($"Articles mapped successfully: {articleDtos.Count}");

                // 載入文章附件
                foreach (var articleDto in articleDtos)
                {
                    var attachments = await _attachmentRepository.GetByArticleIdAsync(articleDto.ArticleId);
                    articleDto.Attachments = _mapper.Map<List<ArticleAttachmentDto>>(attachments);
                }

                return (articleDtos, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetArticlesAsync: {ex}");
                throw;
            }
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
                .Include(a => a.Attachments!.Where(att => (att.Type ?? "") == "image"))
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
        private IQueryable<Article> BuildArticleQuery(
            string? searchKeyword, Guid? authorId, int? status, bool onlyPublic, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Articles
                .AsNoTracking() // 只讀查詢
                                // Status == 0 表示正常，IsPublic == 0 表示公開
                .Where(a => a.Status == 0 && a.IsPublic == 0);

            if (onlyPublic)
            {
                query = query.Where(a => a.Status == 0 && a.IsPublic == 0);
            }
            else
            {
                query = query.Where(a => a.Status != 2);
                if (status.HasValue)
                    query = query.Where(a => a.Status == status.Value);
            }

            if (authorId.HasValue)
                query = query.Where(a => a.AuthorId == authorId.Value);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
                query = query.Where(a =>
                    a.Content.Contains(searchKeyword) ||
                    (a.Author != null && a.Author.DisplayName != null && a.Author.DisplayName.Contains(searchKeyword))
                );
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                query = query.Where(a => a.CreateTime >= dateFrom.Value && a.CreateTime <= dateTo.Value);
            }
            else if (dateFrom.HasValue)
            {
                var next = dateFrom.Value.Date.AddDays(1);
                query = query.Where(a => a.CreateTime >= dateFrom.Value.Date && a.CreateTime < next);
            }

            // 包含作者個人資料的關聯
            return query.Include(a => a.Author!)
                       .ThenInclude(p => p.User);
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

        /// <summary>
        /// 建立一篇新文章，並處理其檔案附件
        /// </summary>
        public async Task<ArticleDto?> CreateArticleWithAttachmentsAsync(Guid authorId, CreateArticleDto dto)
        {
            const int maxImgs = 6;
            const int maxFiles = 6;
            const long maxSize = 5 * 1024 * 1024; // 5MB

            static bool IsImageExt(string fileName)
            {
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
            }
            static bool IsImageByContentType(string? ct)
                => !string.IsNullOrEmpty(ct) && ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            static bool LooksLikeImage(IFormFile f)
                => IsImageByContentType(f.ContentType) || IsImageExt(f.FileName);
            static IEnumerable<IFormFile> Dedupe(IEnumerable<IFormFile> files)
                => files.Where(f => f != null && f.Length > 0)
                        .GroupBy(f => new { f.FileName, f.Length })
                        .Select(g => g.First());

            //先做附件驗證
            var all = new List<IFormFile>();
            if (dto.Images?.Any() == true) all.AddRange(dto.Images);
            if (dto.Files?.Any() == true) all.AddRange(dto.Files);
            if (dto.Attachments?.Any() == true) all.AddRange(dto.Attachments);

            var deduped = Dedupe(all).ToList();
            var images = deduped.Where(LooksLikeImage).ToList();
            var files = deduped.Where(f => !LooksLikeImage(f)).ToList();

            if (images.Count > maxImgs)
                throw new ArgumentException($"圖片最多 {maxImgs} 張");
            if (files.Count > maxFiles)
                throw new ArgumentException($"檔案最多 {maxFiles} 個");
            if (deduped.Any(f => f.Length > maxSize))
                throw new ArgumentException("有檔案超過 5MB 上限");

            //先解析並檢查 Hashtag 上限
            List<Guid> tagIds = new();
            if (dto.SelectedHashtags?.Any() == true)
            {
                tagIds = dto.SelectedHashtags
                    .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .Distinct()
                    .ToList();

                if (tagIds.Count > 6)
                    throw new ArgumentException("最多只能選擇6個標籤");
            }

            // 取得作者並建立文章
            var author = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == authorId);
            if (author == null) return null;

            var article = new Article
            {
                ArticleId = Guid.NewGuid(),
                AuthorId = author.PersonId,
                Content = dto.Content,
                IsPublic = dto.IsPublic,
                CreateTime = DateTime.Now,
                Status = 0,
                PraiseCount = 0,
                CollectCount = 0
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            async Task SaveOneAsync(IFormFile f, string subfolder, string type)
            {
                var path = await _fileService.CreateFileAsync(f, subfolder);
                if (string.IsNullOrEmpty(path))
                    throw new InvalidOperationException($"儲存失敗: {f.FileName}");

                var att = new ArticleAttachment
                {
                    FileId = Guid.NewGuid(),
                    ArticleId = article.ArticleId,
                    FilePath = path,
                    FileName = f.FileName,
                    Type = type,                       // "image" / "file"
                    MimeType = f.ContentType ?? string.Empty
                };
                await _attachmentRepository.AddAsync(att);
            }

            if (dto.Images?.Any() == true)
                foreach (var f in Dedupe(dto.Images))
                    await SaveOneAsync(f, "public/posts/imgs", "image");

            if (dto.Files?.Any() == true)
                foreach (var f in Dedupe(dto.Files))
                    await SaveOneAsync(f, "public/posts/files", "file");

            if (dto.Attachments?.Any() == true)
                foreach (var f in Dedupe(dto.Attachments))
                {
                    var isImg = LooksLikeImage(f);
                    await SaveOneAsync(
                        f,
                        isImg ? "public/posts/imgs" : "public/posts/files",
                        isImg ? "image" : "file"
                    );
                }

            if (tagIds.Count > 0)
                await _articleHashtagRepository.UpdateArticleHashtagsAsync(article.ArticleId, tagIds);

            return await GetArticleAsync(article.ArticleId);
        }



        /// <summary>
        /// 後臺管理員更新文章
        /// </summary>
        public async Task<bool> AdminUpdateArticleContentAsync(Guid id, string content)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);
            if (article == null) return false;

            article.Content = content;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 後臺管理員刪除文章
        /// </summary>
        public async Task<bool> AdminDeleteArticleAsync(Guid id)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);
            if (article == null) return false;

            article.Status = 2;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 獲取文章總數
        /// </summary>
        /// <param name="onlyPublic">是否只計算公開文章</param>
        /// <returns>文章總數</returns>
        public async Task<int> GetTotalArticlesCountAsync()
        {
            return await _context.Articles
                .AsNoTracking()
                .CountAsync();
        }
    }
}