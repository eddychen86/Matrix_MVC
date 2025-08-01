using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 文章標籤關聯資料存取實作
    /// </summary>
    public class ArticleHashtagRepository : BaseRepository<ArticleHashtag>, IArticleHashtagRepository
    {
        public ArticleHashtagRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ArticleHashtag>> GetByArticleIdAsync(Guid articleId)
        {
            return await _dbSet
                .Include(ah => ah.Hashtag)
                .Where(ah => ah.ArticleId == articleId)
                .ToListAsync(); // 修正: 移除對不存在的 CreateTime 的排序
        }

        public async Task<IEnumerable<ArticleHashtag>> GetByHashtagIdAsync(Guid hashtagId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(ah => ah.Article)
                .Where(ah => ah.TagId == hashtagId) // 修正: HashtagId -> TagId
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // 修正: 移除對不存在的 CreateTime 的排序
        }

        public async Task AddArticleHashtagsAsync(Guid articleId, IEnumerable<Guid> hashtagIds)
        {
            var articleHashtags = hashtagIds.Select(hashtagId => new ArticleHashtag
            {
                ArticleId = articleId,
                TagId = hashtagId // 修正: HashtagId -> TagId
                // CreateTime = DateTime.Now // Model 中無此欄位
            });

            await _dbSet.AddRangeAsync(articleHashtags);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByArticleIdAsync(Guid articleId)
        {
            var articleHashtags = await _dbSet
                .Where(ah => ah.ArticleId == articleId)
                .ToListAsync();

            _dbSet.RemoveRange(articleHashtags);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByHashtagIdAsync(Guid hashtagId)
        {
            var articleHashtags = await _dbSet
                .Where(ah => ah.TagId == hashtagId) // 修正: HashtagId -> TagId
                .ToListAsync();

            _dbSet.RemoveRange(articleHashtags);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasTagAsync(Guid articleId, Guid hashtagId)
        {
            return await _dbSet
                .AnyAsync(ah => ah.ArticleId == articleId && ah.TagId == hashtagId); // 修正: HashtagId -> TagId
        }

        public async Task<int> CountTagsByArticleAsync(Guid articleId)
        {
            return await _dbSet.CountAsync(ah => ah.ArticleId == articleId);
        }

        public async Task<int> CountArticlesByTagAsync(Guid hashtagId)
        {
            return await _dbSet.CountAsync(ah => ah.TagId == hashtagId); // 修正: HashtagId -> TagId
        }

        public async Task UpdateArticleHashtagsAsync(Guid articleId, IEnumerable<Guid> hashtagIds)
        {
            // 先刪除現有的標籤關聯
            await DeleteByArticleIdAsync(articleId);

            // 新增新的標籤關聯
            if (hashtagIds.Any())
            {
                await AddArticleHashtagsAsync(articleId, hashtagIds);
            }
        }
    }
}
