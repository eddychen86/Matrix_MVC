using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// NFT 存取介面
    /// </summary>
    public interface INFTRepository : IRepository<NFT>
    {
        /// <summary>
        /// 根據擁有者ID取得NFT清單
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetByOwnerIdAsync(Guid ownerId);
        
        /// <summary>
        /// 根據擁有者ID取得NFT清單，包含擁有者資訊
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetByOwnerIdWithOwnerAsync(Guid ownerId);

        /// <summary>
        /// 根據幣別搜尋NFT
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="currency">幣別</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetByCurrencyAsync(Guid ownerId, string currency);

        /// <summary>
        /// 取得最新收藏的NFT
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="count">數量</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetRecentNFTsAsync(Guid ownerId, int count = 10);

        /// <summary>
        /// 根據價格範圍搜尋NFT
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetByPriceRangeAsync(Guid ownerId, decimal minPrice, decimal maxPrice);

        /// <summary>
        /// 取得NFT詳情，包含擁有者資訊
        /// </summary>
        /// <param name="nftId">NFT ID</param>
        /// <returns>NFT詳情</returns>
        Task<NFT?> GetWithOwnerAsync(Guid nftId);
        
        /// <summary>
        /// 根據檔案名稱搜尋NFT
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="fileName">檔案名稱關鍵字</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> SearchByFileNameAsync(Guid ownerId, string fileName);
        
        /// <summary>
        /// 取得指定時間範圍內收藏的NFT
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="startDate">開始時間</param>
        /// <param name="endDate">結束時間</param>
        /// <returns>NFT清單</returns>
        Task<IEnumerable<NFT>> GetByDateRangeAsync(Guid ownerId, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// 計算用戶NFT總數
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>NFT總數</returns>
        Task<int> GetCountByOwnerIdAsync(Guid ownerId);
        
        /// <summary>
        /// 計算用戶NFT總價值
        /// </summary>
        /// <param name="ownerId">擁有者ID</param>
        /// <param name="currency">指定幣別，null表示所有幣別</param>
        /// <returns>總價值</returns>
        Task<decimal> GetTotalValueByOwnerIdAsync(Guid ownerId, string? currency = null);
    }
}