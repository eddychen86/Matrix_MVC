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
                .FirstOrDefaultAsync(h => h.Name == name);
        }

        public async Task<IEnumerable<Hashtag>> GetPopularTagsAsync(int count = 10)
        {
            return await _dbSet
                .OrderByDescending(h => h.UsageCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hashtag>> SearchTagsAsync(string keyword, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(h => h.Name.Contains(keyword))
                .OrderByDescending(h => h.UsageCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTagUsageCountAsync(Guid hashtagId)
        {
            var hashtag = await _dbSet.FindAsync(hashtagId);
            return hashtag?.UsageCount ?? 0;
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
                        Name = tagName.Trim(),
                        UsageCount = 0,
                        CreateTime = DateTime.Now
                    };
                    await _dbSet.AddAsync(newTag);
                    tags.Add(newTag);
                }
            }

            await _context.SaveChangesAsync();
            return tags;
        }

        public async Task UpdateTagUsageAsync(Guid hashtagId, int usageChange)
        {
            var hashtag = await _dbSet.FindAsync(hashtagId);
            if (hashtag != null)
            {
                hashtag.UsageCount = Math.Max(0, hashtag.UsageCount + usageChange);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Hashtag>> GetRelatedTagsAsync(Guid hashtagId, int count = 5)
        {
            // 這是一個簡化的實作，實際可能需要更複雜的關聯分析
            // 這裡返回使用次數相近的標籤作為相關標籤
            var targetTag = await _dbSet.FindAsync(hashtagId);
            if (targetTag == null) return Enumerable.Empty<Hashtag>();

            return await _dbSet
                .Where(h => h.HashtagId != hashtagId)
                .OrderBy(h => Math.Abs(h.UsageCount - targetTag.UsageCount))
                .Take(count)
                .ToListAsync();
        }

        public async Task CleanupUnusedTagsAsync()
        {
            var unusedTags = await _dbSet
                .Where(h => h.UsageCount == 0)
                .ToListAsync();

            _dbSet.RemoveRange(unusedTags);
            await _context.SaveChangesAsync();
        }
    }
}