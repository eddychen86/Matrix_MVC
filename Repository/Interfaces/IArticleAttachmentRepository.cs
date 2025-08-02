using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 文章附件資料存取介面
    /// </summary>
    public interface IArticleAttachmentRepository : IRepository<ArticleAttachment>
    {
        /// <summary>取得文章的所有附件</summary>
        Task<IEnumerable<ArticleAttachment>> GetByArticleIdAsync(Guid articleId);

        /// <summary>根據檔案類型取得附件</summary>
        Task<IEnumerable<ArticleAttachment>> GetByFileTypeAsync(string fileType, int page = 1, int pageSize = 20);

        /// <summary>取得特定大小範圍的附件</summary>
        Task<IEnumerable<ArticleAttachment>> GetBySizeRangeAsync(long minSize, long maxSize, int page = 1, int pageSize = 20);

        /// <summary>批量刪除文章附件</summary>
        Task DeleteByArticleIdAsync(Guid articleId);

        /// <summary>計算文章附件總大小</summary>
        Task<long> GetTotalSizeByArticleAsync(Guid articleId);

        /// <summary>取得需要清理的過期附件</summary>
        Task<IEnumerable<ArticleAttachment>> GetExpiredAttachmentsAsync(DateTime expiredBefore);

        /// <summary>檢查附件是否屬於指定文章</summary>
        Task<bool> BelongsToArticleAsync(Guid attachmentId, Guid articleId);

        /// <summary>更新附件下載次數</summary>
        Task IncrementDownloadCountAsync(Guid attachmentId);
    }
}