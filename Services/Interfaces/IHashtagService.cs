using Matrix.Models;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 標籤服務介面
    /// </summary>
    public interface IHashtagService
    {
        /// <summary>
        /// 根據標籤名稱取得標籤
        /// </summary>
        /// <param name="name">標籤名稱</param>
        /// <returns>標籤實體</returns>
        Task<Hashtag?> GetHashtagByNameAsync(string name);

        /// <summary>
        /// 取得熱門標籤
        /// </summary>
        /// <param name="count">數量限制</param>
        /// <returns>熱門標籤列表</returns>
        Task<List<Hashtag>> GetPopularHashtagsAsync(int count = 10);

        /// <summary>
        /// 搜尋標籤
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>標籤搜尋結果</returns>
        Task<(List<Hashtag> Hashtags, int TotalCount)> SearchHashtagsAsync(string keyword, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得標籤使用次數
        /// </summary>
        /// <param name="hashtagId">標籤 ID</param>
        /// <returns>使用次數</returns>
        Task<int> GetHashtagUsageCountAsync(Guid hashtagId);

        /// <summary>
        /// 取得所有標籤及其使用次數統計
        /// </summary>
        /// <returns>標籤及使用次數列表</returns>
        Task<List<(Hashtag Tag, int UsageCount)>> GetAllHashtagsWithUsageCountAsync();

        /// <summary>
        /// 批量取得或創建標籤
        /// </summary>
        /// <param name="tagNames">標籤名稱列表</param>
        /// <returns>標籤列表</returns>
        Task<List<Hashtag>> GetOrCreateHashtagsAsync(List<string> tagNames);

        /// <summary>
        /// 清理未使用的標籤
        /// </summary>
        /// <returns>清理是否成功</returns>
        Task<bool> CleanupUnusedHashtagsAsync();
    }
}