using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 點讚收藏資料存取介面
    /// </summary>
    public interface IPraiseCollectRepository : IRepository<PraiseCollect>
    {
        /// <summary>取得用戶對特定文章的點讚收藏記錄</summary>
        Task<PraiseCollect?> GetUserPraiseCollectAsync(Guid userId, Guid articleId);

        /// <summary>取得文章的所有點讚收藏記錄</summary>
        Task<IEnumerable<PraiseCollect>> GetArticlePraiseCollectsAsync(Guid articleId, int page = 1, int pageSize = 20);

        /// <summary>取得用戶的收藏列表</summary>
        Task<IEnumerable<PraiseCollect>> GetUserCollectionsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>取得用戶的點讚列表</summary>
        Task<IEnumerable<PraiseCollect>> GetUserPraisesAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>檢查用戶是否已點讚文章</summary>
        Task<bool> HasUserPraisedAsync(Guid userId, Guid articleId);

        /// <summary>檢查用戶是否已收藏文章</summary>
        Task<bool> HasUserCollectedAsync(Guid userId, Guid articleId);

        /// <summary>計算文章點讚數</summary>
        Task<int> CountPraisesAsync(Guid articleId);

        /// <summary>計算文章收藏數</summary>
        Task<int> CountCollectionsAsync(Guid articleId);

        /// <summary>更新點讚狀態</summary>
        Task UpdatePraiseStatusAsync(Guid userId, Guid articleId, bool isPraised);

        /// <summary>更新收藏狀態</summary>
        Task UpdateCollectStatusAsync(Guid userId, Guid articleId, bool isCollected);
    }
}