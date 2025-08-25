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

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // SQL Server：唯一鍵/索引衝突 = 2601 或 2627
            if (ex.InnerException is Microsoft.Data.SqlClient.SqlException mssql)
                return mssql.Number == 2601 || mssql.Number == 2627;

            // 保險字串判斷（不同 provider / 本地化訊息）
            var msg = ex.InnerException?.Message ?? ex.Message;
            return msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        }

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

        public async Task<IEnumerable<PraiseCollect>> GetUserCollectionsAsync(
                                Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(pc => pc.UserId == userId && pc.Type == CollectType)
                .Include(pc => pc.Article)
                    .ThenInclude(a => a.Author)       // ✅ 一起撈作者
                .Include(pc => pc.Article)
                    .ThenInclude(a => a.Attachments)  // 圖片
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
                await _dbSet.AddAsync(new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    Type = PraiseType,
                    CreateTime = DateTime.UtcNow
                });
            }
            else
            {
                var praise = await _dbSet
                    .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == PraiseType);
                if (praise != null) _dbSet.Remove(praise);
                else return; // 原本就沒資料，當作成功
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 競態下別人已先插入，當作成功即可
                return;
            }
        }



        public async Task UpdateCollectStatusAsync(Guid userId, Guid articleId, bool isCollected)
        {
            if (isCollected)
            {
                await _dbSet.AddAsync(new PraiseCollect
                {
                    UserId = userId,
                    ArticleId = articleId,
                    Type = CollectType,
                    CreateTime = DateTime.UtcNow
                });
            }
            else
            {
                var collect = await _dbSet
                    .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == CollectType);
                if (collect != null) _dbSet.Remove(collect);
                else return; // 原本就沒有，冪等
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 競態下已有人插入，當成功處理
                return;
            }
        }

    }
}
