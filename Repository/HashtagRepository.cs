using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 標籤資料存取實作
    /// </summary>
    public class HashtagRepository : BaseRepository<Hashtag>, IHashtagRepository
    {
        public HashtagRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Hashtag?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .FirstOrDefaultAsync(h => h.Content == name);
        }

        public async Task<IEnumerable<Hashtag>> GetPopularTagsAsync(int count = 10)
        {
            var popularTagIds = await _context.Set<ArticleHashtag>()
                .AsNoTracking() // 只讀統計
                .GroupBy(ah => ah.TagId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Where(h => popularTagIds.Contains(h.TagId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Hashtag>> SearchTagsAsync(string keyword, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .AsNoTracking() // 只讀查詢
                                .Where(h => h.Content.ToLower().Contains(keyword.ToLower()))

                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTagUsageCountAsync(Guid hashtagId)
        {
            return await _context.Set<ArticleHashtag>()
                .AsNoTracking() // 只讀統計
                .CountAsync(ah => ah.TagId == hashtagId);
        }

        public async Task<IEnumerable<Hashtag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames)
        {
            var tags = new List<Hashtag>();
            
            foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var existingTag = await GetByNameAsync(tagName.Trim());
                if (existingTag != null)
                {
                    tags.Add(existingTag);
                }
                else
                {
                    var newTag = new Hashtag
                    {
                        Content = tagName.Trim(),
                        ArticleHashtags = new List<ArticleHashtag>()
                    };
                    await _dbSet.AddAsync(newTag);
                    tags.Add(newTag);
                }
            }

            await _context.SaveChangesAsync();
            return tags;
        }

        public Task UpdateTagUsageAsync(Guid hashtagId, int usageChange)
        {
            // 這個方法在目前的架構下沒有意義，標籤的使用次數由 ArticleHashtag 的記錄決定
            throw new NotImplementedException("Tag usage is determined by ArticleHashtag records, not a direct count.");
        }

        public async Task<IEnumerable<Hashtag>> GetRelatedTagsAsync(Guid hashtagId, int count = 5)
        {
            var articleIdsWithTargetTag = await _context.Set<ArticleHashtag>()
                .AsNoTracking() // 只讀查詢
                .Where(ah => ah.TagId == hashtagId)
                .Select(ah => ah.ArticleId)
                .ToListAsync();

            if (!articleIdsWithTargetTag.Any()) return Enumerable.Empty<Hashtag>();

            var relatedTagIds = await _context.Set<ArticleHashtag>()
                .AsNoTracking() // 只讀統計
                .Where(ah => articleIdsWithTargetTag.Contains(ah.ArticleId) && ah.TagId != hashtagId)
                .GroupBy(ah => ah.TagId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return await _dbSet
                .AsNoTracking() // 只讀查詢
                .Where(h => relatedTagIds.Contains(h.TagId))
                .ToListAsync();
        }

        public async Task CleanupUnusedTagsAsync()
        {
            var usedTagIds = await _context.Set<ArticleHashtag>()
                .AsNoTracking() // 只讀查詢
                .Select(ah => ah.TagId)
                .Distinct()
                .ToListAsync();

            var unusedTags = await _dbSet
                .Where(h => !usedTagIds.Contains(h.TagId))
                .ToListAsync();

            if (unusedTags.Any())
            {
                _dbSet.RemoveRange(unusedTags);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<(Hashtag Tag, int UsageCount)>> GetAllTagsWithUsageCountAsync()
        {
            var result = await _dbSet
                .AsNoTracking()
                .Select(h => new
                {
                    Tag = h,
                    UsageCount = _context.Set<ArticleHashtag>()
                        .Count(ah => ah.TagId == h.TagId)
                })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();

            return result.Select(x => (x.Tag, x.UsageCount));
        }
    }
}
