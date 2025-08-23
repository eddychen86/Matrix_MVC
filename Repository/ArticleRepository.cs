using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 文章資料存取實作
    /// </summary>
    public class ArticleRepository : BaseRepository<Article>, IArticleRepository
    {
        public ArticleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Article>> GetByAuthorIdAsync(Guid authorId)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetPublicArticlesAsync(int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .Where(a => a.IsPublic == 0) // 公開文章
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string keyword, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .Where(a => a.IsPublic == 0 && 
                           a.Content.Contains(keyword)) // 修正: 移除 Title，只搜尋 Content
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetHotArticlesAsync(int count = 10)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .Where(a => a.IsPublic == 0)
                .OrderByDescending(a => a.PraiseCount) // 修正: ViewCount -> PraiseCount
                .ThenByDescending(a => a.CreateTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleWithAttachmentsAsync(Guid articleId)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        }

        public async Task<Article?> GetArticleWithRepliesAsync(Guid articleId)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Author)
                .Include(a => a.Replies!) // 修正: 使用空值容許運算子
                    .ThenInclude(r => r.User) // 修正: r.Author -> r.User
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        }

        public async Task<IEnumerable<Article>> GetCollectedArticlesByUserAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _context.PraiseCollects
                .AsNoTracking() // 只讀查詢
                .Where(pc => pc.UserId == userId && pc.Type == 1) // 假設 Type 1 是收藏
                .Join(_dbSet, 
                      pc => pc.ArticleId, 
                      a => a.ArticleId, 
                      (pc, a) => a)
                .Include(a => a.Author)
                .Include(a => a.Attachments)
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(int ViewCount, int LikeCount, int ReplyCount)> GetArticleStatsAsync(Guid articleId)
        {
            var article = await _dbSet
                .AsNoTracking() // 只讀查詢
                .Include(a => a.Replies)
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);

            if (article == null)
                return (0, 0, 0);

            var likeCount = await _context.PraiseCollects
                .AsNoTracking() // 只讀統計
                .CountAsync(pc => pc.ArticleId == articleId && pc.Type == 0); // 假設 Type 0 是按讚

            return (0, likeCount, article.Replies?.Count ?? 0); // 修正: 移除 ViewCount
        }

        public async Task<bool> IncreasePraiseCountAsync(Guid articleId)
        {
            return await UpdateArticleCountAsync(articleId, a => a.PraiseCount++);
        }

        public async Task<bool> DecreasePraiseCountAsync(Guid articleId)
        {
            return await UpdateArticleCountAsync(articleId, a => { if (a.PraiseCount > 0) a.PraiseCount--; });
        }

        public async Task<bool> IncreaseCollectCountAsync(Guid articleId)
        {
            return await UpdateArticleCountAsync(articleId, a => a.CollectCount++);
        }

        public async Task<bool> DecreaseCollectCountAsync(Guid articleId)
        {
            return await UpdateArticleCountAsync(articleId, a => { if (a.CollectCount > 0) a.CollectCount--; });
        }

        /// <summary>
        /// 安全更新文章計數，使用樂觀併發控制
        /// </summary>
        private async Task<bool> UpdateArticleCountAsync(Guid articleId, Action<Article> updateAction)
        {
            const int maxRetries = 3;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var article = await _dbSet.FindAsync(articleId);
                    if (article == null) return false;
                    
                    updateAction(article);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // 併發衝突，重試
                    if (attempt == maxRetries - 1)
                        throw; // 最後一次重試失敗，拋出異常
                    
                    await Task.Delay(100 * (attempt + 1)); // 指數延遲
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 使用原子資料庫操作更新計數（最安全）
        /// </summary>
        public async Task<bool> IncreasePraiseCountAtomicAsync(Guid articleId)
        {
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Articles SET PraiseCount = PraiseCount + 1 WHERE ArticleId = {articleId}"
            );
            return rowsAffected > 0;
        }
        
        /// <summary>
        /// 使用原子資料庫操作更新計數（最安全）
        /// </summary>
        public async Task<bool> DecreasePraiseCountAtomicAsync(Guid articleId)
        {
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Articles SET PraiseCount = CASE WHEN PraiseCount > 0 THEN PraiseCount - 1 ELSE 0 END WHERE ArticleId = {articleId}"
            );
            return rowsAffected > 0;
        }
        
        /// <summary>
        /// 使用原子資料庫操作更新收藏計數（最安全）
        /// </summary>
        public async Task<bool> IncreaseCollectCountAtomicAsync(Guid articleId)
        {
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Articles SET CollectCount = CollectCount + 1 WHERE ArticleId = {articleId}"
            );
            return rowsAffected > 0;
        }
        
        /// <summary>
        /// 使用原子資料庫操作更新收藏計數（最安全）
        /// </summary>
        public async Task<bool> DecreaseCollectCountAtomicAsync(Guid articleId)
        {
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Articles SET CollectCount = CASE WHEN CollectCount > 0 THEN CollectCount - 1 ELSE 0 END WHERE ArticleId = {articleId}"
            );
            return rowsAffected > 0;
        }

        public async Task<(List<ArticleDto> Items, int TotalCount)> GetArticlesWithUserStateAsync(int page, int pageSize, Guid? personId)
        {
            const int PraiseType = 0;
            const int CollectType = 1;

            var baseQuery = _context.Articles
                .AsNoTracking()
                .Where(a => a.Status == 0 && a.IsPublic == 0);

            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(a => a.CreateTime)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    AuthorId = a.AuthorId,
                    Content = a.Content,
                    CreateTime = a.CreateTime,
                    Status = a.Status,
                    IsPublic = a.IsPublic,
                    PraiseCount = a.PraiseCount,
                    CollectCount = a.CollectCount,

                    // 關鍵：當前使用者狀態（未登入 = false）
                    IsPraised = personId != null && _context.PraiseCollects
                        .Any(pc => pc.ArticleId == a.ArticleId
                                && pc.UserId == personId.Value
                                && pc.Type == PraiseType),

                    IsCollected = personId != null && _context.PraiseCollects
                        .Any(pc => pc.ArticleId == a.ArticleId
                                && pc.UserId == personId.Value
                                && pc.Type == CollectType),
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }


    }
}
