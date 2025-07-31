using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 文章標籤關聯資料存取介面
    /// </summary>
    public interface IArticleHashtagRepository : IRepository<ArticleHashtag>
    {
        /// <summary>取得文章的所有標籤</summary>
        Task<IEnumerable<ArticleHashtag>> GetByArticleIdAsync(Guid articleId);

        /// <summary>取得標籤的所有文章</summary>
        Task<IEnumerable<ArticleHashtag>> GetByHashtagIdAsync(Guid hashtagId, int page = 1, int pageSize = 20);

        /// <summary>批量新增文章標籤</summary>
        Task AddArticleHashtagsAsync(Guid articleId, IEnumerable<Guid> hashtagIds);

        /// <summary>刪除文章的所有標籤</summary>
        Task DeleteByArticleIdAsync(Guid articleId);

        /// <summary>刪除標籤的所有關聯</summary>
        Task DeleteByHashtagIdAsync(Guid hashtagId);

        /// <summary>檢查文章是否有特定標籤</summary>
        Task<bool> HasTagAsync(Guid articleId, Guid hashtagId);

        /// <summary>取得文章標籤數量</summary>
        Task<int> CountTagsByArticleAsync(Guid articleId);

        /// <summary>取得標籤文章數量</summary>
        Task<int> CountArticlesByTagAsync(Guid hashtagId);

        /// <summary>更新文章標籤（先刪除後新增）</summary>
        Task UpdateArticleHashtagsAsync(Guid articleId, IEnumerable<Guid> hashtagIds);
    }
}