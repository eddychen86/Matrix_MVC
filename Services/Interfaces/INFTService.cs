namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// NFT 服務介面
    /// </summary>
    public interface INFTService
    {
        /// <summary>
        /// 取得用戶的所有 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> GetUserNFTsAsync(Guid ownerId);

        /// <summary>
        /// 取得用戶的所有 NFT，包含擁有者資訊
        /// </summary>
        Task<IEnumerable<NFTDto>> GetUserNFTsWithOwnerAsync(Guid ownerId);

        /// <summary>
        /// 根據 NFT ID 取得 NFT 詳情
        /// </summary>
        Task<NFTDto?> GetNFTByIdAsync(Guid nftId);

        /// <summary>
        /// 創建新的 NFT
        /// </summary>
        Task<ReturnType<NFTDto>> CreateNFTAsync(CreateNFTDto createDto);

        /// <summary>
        /// 更新 NFT 資訊
        /// </summary>
        Task<ReturnType<bool>> UpdateNFTAsync(Guid nftId, UpdateNFTDto updateDto);

        /// <summary>
        /// 刪除 NFT
        /// </summary>
        Task<ReturnType<bool>> DeleteNFTAsync(Guid nftId, Guid ownerId);

        /// <summary>
        /// 根據幣別取得 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> GetNFTsByCurrencyAsync(Guid ownerId, string currency);

        /// <summary>
        /// 取得最近收藏的 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> GetRecentNFTsAsync(Guid ownerId, int count = 10);

        /// <summary>
        /// 根據價格範圍搜尋 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> GetNFTsByPriceRangeAsync(Guid ownerId, decimal minPrice, decimal maxPrice);

        /// <summary>
        /// 根據檔案名稱搜尋 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> SearchNFTsByFileNameAsync(Guid ownerId, string fileName);

        /// <summary>
        /// 根據日期範圍取得 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> GetNFTsByDateRangeAsync(Guid ownerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// 進階搜尋 NFT
        /// </summary>
        Task<IEnumerable<NFTDto>> SearchNFTsAsync(NFTSearchDto searchDto);

        /// <summary>
        /// 取得用戶 NFT 統計資料
        /// </summary>
        Task<NFTStatsDto> GetUserNFTStatsAsync(Guid ownerId);

        /// <summary>
        /// 取得用戶 NFT 總數
        /// </summary>
        Task<int> GetUserNFTCountAsync(Guid ownerId);

        /// <summary>
        /// 取得用戶 NFT 總價值
        /// </summary>
        Task<decimal> GetUserNFTTotalValueAsync(Guid ownerId, string? currency = null);

        /// <summary>
        /// 驗證用戶是否為 NFT 的擁有者
        /// </summary>
        Task<bool> IsOwnerAsync(Guid nftId, Guid ownerId);

        /// <summary>
        /// 批量導入 NFT
        /// </summary>
        Task<ReturnType<int>> BulkImportNFTsAsync(IEnumerable<CreateNFTDto> nfts);

        /// <summary>
        /// 批量刪除 NFT
        /// </summary>
        Task<ReturnType<int>> BulkDeleteNFTsAsync(IEnumerable<Guid> nftIds, Guid ownerId);
    }
}