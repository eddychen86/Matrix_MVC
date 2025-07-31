using System.Linq.Expressions;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 基底 Repository 介面，提供通用的 CRUD 操作
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>根據 ID 取得實體</summary>
        Task<T?> GetByIdAsync<TKey>(TKey id);

        /// <summary>根據條件取得實體</summary>
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>取得所有實體</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>根據條件取得實體集合</summary>
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>新增實體</summary>
        Task<T> AddAsync(T entity);

        /// <summary>更新實體</summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>刪除實體</summary>
        Task DeleteAsync(T entity);

        /// <summary>根據 ID 刪除實體</summary>
        Task DeleteByIdAsync<TKey>(TKey id);

        /// <summary>檢查實體是否存在</summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>取得符合條件的實體數量</summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>儲存變更</summary>
        Task<int> SaveChangesAsync();
    }
}