using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 文章附件資料存取實作
    /// </summary>
    public class ArticleAttachmentRepository : BaseRepository<ArticleAttachment>, IArticleAttachmentRepository
    {
        public ArticleAttachmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ArticleAttachment>> GetByArticleIdAsync(Guid articleId)
        {
            return await _dbSet
                .Where(aa => aa.ArticleId == articleId)
                .OrderBy(aa => aa.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticleAttachment>> GetByFileTypeAsync(string fileType, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(aa => aa.Article)
                .Where(aa => aa.FileType == fileType)
                .OrderByDescending(aa => aa.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticleAttachment>> GetBySizeRangeAsync(long minSize, long maxSize, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(aa => aa.Article)
                .Where(aa => aa.FileSize >= minSize && aa.FileSize <= maxSize)
                .OrderByDescending(aa => aa.FileSize)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task DeleteByArticleIdAsync(Guid articleId)
        {
            var attachments = await _dbSet
                .Where(aa => aa.ArticleId == articleId)
                .ToListAsync();

            _dbSet.RemoveRange(attachments);
            await _context.SaveChangesAsync();
        }

        public async Task<long> GetTotalSizeByArticleAsync(Guid articleId)
        {
            return await _dbSet
                .Where(aa => aa.ArticleId == articleId)
                .SumAsync(aa => aa.FileSize);
        }

        public async Task<IEnumerable<ArticleAttachment>> GetExpiredAttachmentsAsync(DateTime expiredBefore)
        {
            return await _dbSet
                .Where(aa => aa.CreateTime < expiredBefore)
                .ToListAsync();
        }

        public async Task<bool> BelongsToArticleAsync(Guid attachmentId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(aa => aa.AttachmentId == attachmentId && aa.ArticleId == articleId);
        }

        public async Task IncrementDownloadCountAsync(Guid attachmentId)
        {
            var attachment = await _dbSet.FindAsync(attachmentId);
            if (attachment != null)
            {
                attachment.DownloadCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}