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
                .ToListAsync(); // 修正: 移除對不存在的 CreateTime 的排序
        }

        public async Task<IEnumerable<ArticleAttachment>> GetByFileTypeAsync(string fileType, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(aa => aa.Article)
                .Where(aa => aa.Type == fileType) // 修正: FileType -> Type
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // 修正: 移除對不存在的 CreateTime 的排序
        }

        public Task<IEnumerable<ArticleAttachment>> GetBySizeRangeAsync(long minSize, long maxSize, int page = 1, int pageSize = 20)
        {
            // 注意: Model 中沒有 FileSize，無法實現此功能
            throw new NotImplementedException("ArticleAttachment Model does not contain FileSize.");
        }

        public async Task DeleteByArticleIdAsync(Guid articleId)
        {
            var attachments = await _dbSet
                .Where(aa => aa.ArticleId == articleId)
                .ToListAsync();

            _dbSet.RemoveRange(attachments);
            await _context.SaveChangesAsync();
        }

        public Task<long> GetTotalSizeByArticleAsync(Guid articleId)
        {
            // 注意: Model 中沒有 FileSize，無法實現此功能
            throw new NotImplementedException("ArticleAttachment Model does not contain FileSize.");
        }

        public Task<IEnumerable<ArticleAttachment>> GetExpiredAttachmentsAsync(DateTime expiredBefore)
        {
            // 注意: Model 中沒有 CreateTime，無法實現此功能
            throw new NotImplementedException("ArticleAttachment Model does not contain CreateTime.");
        }

        public async Task<bool> BelongsToArticleAsync(Guid attachmentId, Guid articleId)
        {
            return await _dbSet
                .AnyAsync(aa => aa.FileId == attachmentId && aa.ArticleId == articleId); // 修正: AttachmentId -> FileId
        }

        public Task IncrementDownloadCountAsync(Guid attachmentId)
        {
            // 注意: Model 中沒有 DownloadCount，無法實現此功能
            throw new NotImplementedException("ArticleAttachment Model does not contain DownloadCount.");
        }
    }
}
