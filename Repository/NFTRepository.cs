using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// NFT 存取實作
    /// </summary>
    public class NFTRepository : BaseRepository<NFT>, INFTRepository
    {
        public NFTRepository(ApplicationDbContext context) : base(context) { }

        /// <summary>
        /// 根據擁有者ID取得NFT清單
        /// </summary>
        public async Task<IEnumerable<NFT>> GetByOwnerIdAsync(Guid ownerId)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId)
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據擁有者ID取得NFT清單，包含擁有者資訊
        /// </summary>
        public async Task<IEnumerable<NFT>> GetByOwnerIdWithOwnerAsync(Guid ownerId)
        {
            return await _dbSet
                .Include(n => n.Owner)
                .Where(n => n.OwnerId == ownerId)
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據幣別搜尋NFT
        /// </summary>
        public async Task<IEnumerable<NFT>> GetByCurrencyAsync(Guid ownerId, string currency)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId && n.Currency == currency)
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 取得最新收藏的NFT
        /// </summary>
        public async Task<IEnumerable<NFT>> GetRecentNFTsAsync(Guid ownerId, int count = 10)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId)
                .OrderByDescending(n => n.CollectTime)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// 根據價格範圍搜尋NFT
        /// </summary>
        public async Task<IEnumerable<NFT>> GetByPriceRangeAsync(Guid ownerId, decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId && n.Price >= minPrice && n.Price <= maxPrice)
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 取得NFT詳情，包含擁有者資訊
        /// </summary>
        public async Task<NFT?> GetWithOwnerAsync(Guid nftId)
        {
            return await _dbSet
                .Include(n => n.Owner)
                .FirstOrDefaultAsync(n => n.NftId == nftId);
        }

        /// <summary>
        /// 根據檔案名稱搜尋NFT
        /// </summary>
        public async Task<IEnumerable<NFT>> SearchByFileNameAsync(Guid ownerId, string fileName)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId && n.FileName.Contains(fileName))
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 取得指定時間範圍內收藏的NFT
        /// </summary>
        public async Task<IEnumerable<NFT>> GetByDateRangeAsync(Guid ownerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId && n.CollectTime >= startDate && n.CollectTime <= endDate)
                .OrderByDescending(n => n.CollectTime)
                .ToListAsync();
        }

        /// <summary>
        /// 計算用戶NFT總數
        /// </summary>
        public async Task<int> GetCountByOwnerIdAsync(Guid ownerId)
        {
            return await _dbSet
                .Where(n => n.OwnerId == ownerId)
                .CountAsync();
        }

        /// <summary>
        /// 計算用戶NFT總價值
        /// </summary>
        public async Task<decimal> GetTotalValueByOwnerIdAsync(Guid ownerId, string? currency = null)
        {
            var query = _dbSet.Where(n => n.OwnerId == ownerId);
            
            if (!string.IsNullOrEmpty(currency))
            {
                query = query.Where(n => n.Currency == currency);
            }
            
            return await query.SumAsync(n => n.Price);
        }
    }
}