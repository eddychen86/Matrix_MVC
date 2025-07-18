namespace Matrix.Services.Interfaces;

/// <summary>
/// 狀態管理介面
/// 定義狀態管理相關功能
/// </summary>
/// <typeparam name="TKey">主鍵類型</typeparam>
public interface IStatusManageable<TKey>
{
    /// <summary>
    /// 更新實體狀態
    /// </summary>
    /// <param name="id">實體 ID</param>
    /// <param name="status">新狀態</param>
    /// <returns>更新是否成功</returns>
    Task<bool> UpdateStatusAsync(TKey id, int status);
}