using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 點讚收藏資料存取實作
    /// </summary>
    public class PraiseCollectRepository : BaseRepository<PraiseCollect>, IPraiseCollectRepository
    {
        public PraiseCollectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PraiseCollect?> GetUserPraiseCollectAsync(Guid userId, Guid articleId)
        {
            return await _dbSet
                .Include(pc => pc.User)
                .Include(pc => pc.Article)
                .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId);
        }

        public async Task<IEnumerable<PraiseCollect>> GetArticlePraiseCollectsAsync(Guid articleId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(pc => pc.User)
                .Where(pc => pc.ArticleId == articleId)
                .OrderByDescending(pc => pc.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PraiseCollect>> GetUserCollectionsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(pc => pc.Article)
                .Where(pc => pc.UserId == userId && pc.IsCollected)
                .OrderByDescending(pc => pc.CollectTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PraiseCollect>> GetUserPraisesAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(pc => pc.Article)
                .Where(pc => pc.UserId == userId && pc.IsPraised)
                .OrderByDescending(pc => pc.PraiseTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> HasUserPraisedAsync(Guid userId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.IsPraised);
        }

        public async Task<bool> HasUserCollectedAsync(Guid userId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.IsCollected);
        }

        public async Task<int> CountPraisesAsync(Guid articleId)
        {
            return await _dbSet
                .CountAsync(pc => pc.ArticleId == articleId && pc.IsPraised);
        }

        public async Task<int> CountCollectionsAsync(Guid articleId)
        {
            return await _dbSet
                .CountAsync(pc => pc.ArticleId == articleId && pc.IsCollected);
        }

        public async Task UpdatePraiseStatusAsync(Guid userId, Guid articleId, bool isPraised)
        {
            var praiseCollect = await _dbSet
                .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId);

            if (praiseCollect == null && isPraised)
            {
                praiseCollect = new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    IsPraised = true,
                    PraiseTime = DateTime.Now,
                    CreateTime = DateTime.Now
                };
                await _dbSet.AddAsync(praiseCollect);
            }
            else if (praiseCollect != null)
            {
                praiseCollect.IsPraised = isPraised;
                praiseCollect.PraiseTime = isPraised ? DateTime.Now : null;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCollectStatusAsync(Guid userId, Guid articleId, bool isCollected)
        {
            var praiseCollect = await _dbSet
                .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId);

            if (praiseCollect == null && isCollected)
            {
                praiseCollect = new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    IsCollected = true,
                    CollectTime = DateTime.Now,
                    CreateTime = DateTime.Now
                };
                await _dbSet.AddAsync(praiseCollect);
            }
            else if (praiseCollect != null)
            {
                praiseCollect.IsCollected = isCollected;
                praiseCollect.CollectTime = isCollected ? DateTime.Now : null;
            }

            await _context.SaveChangesAsync();
        }
    }
}