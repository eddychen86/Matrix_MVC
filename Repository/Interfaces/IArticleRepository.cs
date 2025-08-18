using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 文章資料存取介面
    /// </summary>
    public interface IArticleRepository : IRepository<Article>
    {
        /// <summary>根據作者ID取得文章列表</summary>
        Task<IEnumerable<Article>> GetByAuthorIdAsync(Guid authorId);

        /// <summary>取得公開文章列表</summary>
        Task<IEnumerable<Article>> GetPublicArticlesAsync(int page = 1, int pageSize = 20);

        /// <summary>根據標題或內容搜尋文章</summary>
        Task<IEnumerable<Article>> SearchArticlesAsync(string keyword, int page = 1, int pageSize = 20);

        /// <summary>取得熱門文章</summary>
        Task<IEnumerable<Article>> GetHotArticlesAsync(int count = 10);

        /// <summary>取得文章及其附件</summary>
        Task<Article?> GetArticleWithAttachmentsAsync(Guid articleId);

        /// <summary>取得文章及其回覆</summary>
        Task<Article?> GetArticleWithRepliesAsync(Guid articleId);

        /// <summary>取得用戶收藏的文章</summary>
        Task<IEnumerable<Article>> GetCollectedArticlesByUserAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得文章統計數據</summary>
        Task<(int ViewCount, int LikeCount, int ReplyCount)> GetArticleStatsAsync(Guid articleId);

        Task<bool> IncreasePraiseCountAsync(Guid articleId);
        Task<bool> DecreasePraiseCountAsync(Guid articleId);
        Task<bool> IncreaseCollectCountAsync(Guid articleId);
        Task<bool> DecreaseCollectCountAsync(Guid articleId);
        
        // 原子操作方法（併發安全）
        Task<bool> IncreasePraiseCountAtomicAsync(Guid articleId);
        Task<bool> DecreasePraiseCountAtomicAsync(Guid articleId);
        Task<bool> IncreaseCollectCountAtomicAsync(Guid articleId);
        Task<bool> DecreaseCollectCountAtomicAsync(Guid articleId);
    }
}