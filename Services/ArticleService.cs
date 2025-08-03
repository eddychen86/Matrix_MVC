namespace Matrix.Services
{
    /// <summary>
    /// 文章服務
    /// </summary>
    public class ArticleService(ApplicationDbContext _context) : IArticleService
    {
        /// <summary>
        /// 根據ID獲取文章
        /// </summary>
        public async Task<ArticleDto?> GetArticleAsync(Guid id)
        {
            var article = await _context.Articles
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Replies!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            return article == null ? null : MapToArticleDto(article, true);
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
            int page = 1, int pageSize = 20, string? searchKeyword = null, Guid? authorId = null)
        {
            var query = BuildArticleQuery(searchKeyword, authorId);
            var totalCount = await query.CountAsync();

            var articles = await query
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToSimpleArticleDto(a))
                .ToListAsync();

            return (articles, totalCount);
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

            return await _context.Articles
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Where(a => a.Status == 0 && a.IsPublic == 0 && a.CreateTime >= sinceDate)
                .OrderByDescending(a => a.PraiseCount + a.CollectCount)
                .Take(limit)
                .Select(a => MapToSimpleArticleDto(a))
                .ToListAsync();
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

        // 私有輔助方法
        private IQueryable<Article> BuildArticleQuery(string? searchKeyword, Guid? authorId)
        {
            var query = _context.Articles
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Where(a => a.Status == 0 && a.IsPublic == 0);

            if (authorId.HasValue)
                query = query.Where(a => a.Author != null && a.Author.UserId == authorId.Value);

            if (!string.IsNullOrEmpty(searchKeyword))
                query = query.Where(a => a.Content.Contains(searchKeyword) ||
                                       (a.Author != null && a.Author.DisplayName != null && a.Author.DisplayName.Contains(searchKeyword)));

            return query;
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

        private static PersonDto? MapToPersonDto(Person? person)
        {
            return person == null ? null : new PersonDto
            {
                PersonId = person.PersonId,
                UserId = person.UserId,
                DisplayName = person.DisplayName,
                AvatarPath = person.AvatarPath,
                IsPrivate = person.IsPrivate
            };
        }

        private static ArticleDto MapToSimpleArticleDto(Article a)
        {
            return new ArticleDto
            {
                ArticleId = a.ArticleId,
                AuthorId = a.AuthorId,
                Content = a.Content,
                IsPublic = a.IsPublic,
                Status = a.Status,
                CreateTime = a.CreateTime,
                PraiseCount = a.PraiseCount,
                CollectCount = a.CollectCount,
                Author = MapToPersonDto(a.Author),
                Replies = []
            };
        }

        private static ArticleDto MapToArticleDto(Article article, bool includeReplies = false)
        {
            var dto = MapToSimpleArticleDto(article);

            if (includeReplies && article.Replies != null)
            {
                dto.Replies = article.Replies.Select(r => new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    ArticleId = r.ArticleId,
                    AuthorId = r.UserId,
                    Content = r.Content,
                    CreateTime = r.ReplyTime,
                    Author = MapToPersonDto(r.User)
                }).ToList();
            }

            return dto;
        }
    }
}