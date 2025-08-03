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
    }
}
