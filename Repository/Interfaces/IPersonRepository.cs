using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 個人資料存取介面
    /// </summary>
    public interface IPersonRepository : IRepository<Person>
    {
        /// <summary>根據用戶ID取得個人資料</summary>
        Task<Person?> GetByUserIdAsync(Guid userId);

        /// <summary>根據顯示名稱搜尋個人資料</summary>
        Task<IEnumerable<Person>> SearchByDisplayNameAsync(string displayName);

        /// <summary>取得公開的個人資料列表</summary>
        Task<IEnumerable<Person>> GetPublicPersonsAsync(int page = 1, int pageSize = 20);

        /// <summary>根據錢包地址取得個人資料</summary>
        Task<Person?> GetByWalletAddressAsync(string walletAddress);

        /// <summary>根據用戶ID取得個人資料，包含使用者和文章資訊</summary>
        Task<Person?> GetByUserIdWithIncludesAsync(Guid userId);

        /// <summary>根據用戶ID取得個人資料（用於更新操作，啟用變更追蹤）</summary>
        Task<Person?> GetByUserIdForUpdateAsync(Guid userId);
    }
}