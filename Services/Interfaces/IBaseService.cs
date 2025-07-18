namespace Matrix.Services.Interfaces;

/// <summary>
/// 基礎服務介面
/// </summary>
public interface IBaseService<T, TKey> where T : class
{
    /// <summary>
    /// 根據 ID 獲取實體
    /// </summary>
    Task<T?> GetAsync(TKey id);

    /// <summary>
    /// 建立新實體
    /// </summary>
    Task<bool> CreateAsync(T entity);

    /// <summary>
    /// 更新實體
    /// </summary>
    Task<bool> UpdateAsync(TKey id, T entity);

    /// <summary>
    /// 刪除實體
    /// </summary>
    Task<bool> DeleteAsync(TKey id);

    /// <summary>
    /// 分頁查詢實體列表
    /// </summary>
    Task<(List<T> Items, int TotalCount)> GetPagedAsync(int page = 1, int pageSize = 20);
}