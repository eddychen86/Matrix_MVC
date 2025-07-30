using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 標籤資料存取介面
    /// </summary>
    public interface IHashtagRepository : IRepository<Hashtag>
    {
        /// <summary>根據標籤名稱取得標籤</summary>
        Task<Hashtag?> GetByNameAsync(string name);

        /// <summary>取得熱門標籤</summary>
        Task<IEnumerable<Hashtag>> GetPopularTagsAsync(int count = 10);

        /// <summary>搜尋標籤</summary>
        Task<IEnumerable<Hashtag>> SearchTagsAsync(string keyword, int page = 1, int pageSize = 20);

        /// <summary>取得標籤的使用次數</summary>
        Task<int> GetTagUsageCountAsync(Guid hashtagId);

        /// <summary>批量取得或創建標籤</summary>
        Task<IEnumerable<Hashtag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames);

        /// <summary>更新標籤使用次數</summary>
        Task UpdateTagUsageAsync(Guid hashtagId, int usageChange);

        /// <summary>取得相關標籤</summary>
        Task<IEnumerable<Hashtag>> GetRelatedTagsAsync(Guid hashtagId, int count = 5);

        /// <summary>清理未使用的標籤</summary>
        Task CleanupUnusedTagsAsync();
    }
}