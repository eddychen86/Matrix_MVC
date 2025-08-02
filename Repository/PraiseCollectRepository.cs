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
        private const int PraiseType = 0;
        private const int CollectType = 1;

        public PraiseCollectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PraiseCollect?> GetUserPraiseCollectAsync(Guid userId, Guid articleId)
        {
            // 這個方法在目前的 Model 設計下意義不大，因為一個用戶可以同時對一篇文章點讚和收藏（兩筆記錄）
            // 但為了符合介面，我們回傳第一筆找到的記錄
            return await _dbSet
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
                .Where(pc => pc.UserId == userId && pc.Type == CollectType)
                .OrderByDescending(pc => pc.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PraiseCollect>> GetUserPraisesAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(pc => pc.Article)
                .Where(pc => pc.UserId == userId && pc.Type == PraiseType)
                .OrderByDescending(pc => pc.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> HasUserPraisedAsync(Guid userId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == PraiseType);
        }

        public async Task<bool> HasUserCollectedAsync(Guid userId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == CollectType);
        }

        public async Task<int> CountPraisesAsync(Guid articleId)
        {
            return await _dbSet
                .CountAsync(pc => pc.ArticleId == articleId && pc.Type == PraiseType);
        }

        public async Task<int> CountCollectionsAsync(Guid articleId)
        {
            return await _dbSet
                .CountAsync(pc => pc.ArticleId == articleId && pc.Type == CollectType);
        }

        public async Task UpdatePraiseStatusAsync(Guid userId, Guid articleId, bool isPraised)
        {
            if (isPraised)
            {
                // 如果已經按過讚，就什麼都不做
                if (await HasUserPraisedAsync(userId, articleId)) return;

                var praise = new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    Type = PraiseType,
                    CreateTime = DateTime.Now
                };
                await _dbSet.AddAsync(praise);
            }
            else
            {
                // 找到對應的讚並刪除
                var praise = await _dbSet
                    .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == PraiseType);
                
                if (praise != null)
                {
                    _dbSet.Remove(praise);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCollectStatusAsync(Guid userId, Guid articleId, bool isCollected)
        {
            if (isCollected)
            {
                // 如果已經收藏過，就什麼都不做
                if (await HasUserCollectedAsync(userId, articleId)) return;

                var collect = new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    Type = CollectType,
                    CreateTime = DateTime.Now
                };
                await _dbSet.AddAsync(collect);
            }
            else
            {
                // 找到對應的收藏並刪除
                var collect = await _dbSet
                    .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == CollectType);

                if (collect != null)
                {
                    _dbSet.Remove(collect);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
