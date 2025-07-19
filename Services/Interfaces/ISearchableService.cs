namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 可搜尋服務介面
    /// 定義搜尋相關功能
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface ISearchableService<T> where T : class
    {
        /// <summary>
        /// 搜尋實體
        /// </summary>
        /// <param name="keyword">搜尋關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>搜尋結果和總數量</returns>
        Task<(List<T> Items, int TotalCount)> SearchAsync(
            string? keyword = null,
            int page = 1,
            int pageSize = 20);
    }
}