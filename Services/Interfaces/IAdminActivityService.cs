using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 管理員活動記錄服務介面
    /// 定義管理員活動追蹤相關的業務邏輯操作
    /// </summary>
    public interface IAdminActivityService
    {
        #region 活動記錄操作

        /// <summary>
        /// 記錄管理員活動
        /// </summary>
        /// <param name="activityDto">活動記錄資料</param>
        /// <returns>創建的活動記錄 ID</returns>
        Task<Guid> LogActivityAsync(CreateActivityLogDto activityDto);

        /// <summary>
        /// 記錄管理員登入
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="adminName">管理員名稱</param>
        /// <param name="role">管理員角色</param>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="userAgent">用戶代理</param>
        /// <returns>活動記錄 ID</returns>
        Task<Guid> LogLoginAsync(Guid userId, string adminName, int role, string ipAddress, string userAgent);

        /// <summary>
        /// 記錄管理員登出
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="ipAddress">IP 地址</param>
        /// <returns>是否成功更新登出時間</returns>
        Task<bool> LogLogoutAsync(Guid userId, string ipAddress);

        /// <summary>
        /// 記錄頁面訪問
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="adminName">管理員名稱</param>
        /// <param name="role">管理員角色</param>
        /// <param name="pagePath">頁面路徑</param>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="userAgent">用戶代理</param>
        /// <param name="duration">停留時間（秒）</param>
        /// <returns>活動記錄 ID</returns>
        Task<Guid> LogPageVisitAsync(Guid userId, string adminName, int role, string pagePath, 
            string ipAddress, string userAgent, int duration = 0);

        /// <summary>
        /// 記錄管理員操作（創建、更新、刪除等）
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="adminName">管理員名稱</param>
        /// <param name="role">管理員角色</param>
        /// <param name="actionType">操作類型</param>
        /// <param name="actionDescription">操作描述</param>
        /// <param name="pagePath">頁面路徑</param>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="userAgent">用戶代理</param>
        /// <param name="isSuccessful">是否成功</param>
        /// <param name="errorMessage">錯誤訊息</param>
        /// <returns>活動記錄 ID</returns>
        Task<Guid> LogActionAsync(Guid userId, string adminName, int role, string actionType, 
            string actionDescription, string pagePath, string ipAddress, string userAgent, 
            bool isSuccessful = true, string? errorMessage = null);

        /// <summary>
        /// 記錄錯誤活動
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="adminName">管理員名稱</param>
        /// <param name="role">管理員角色</param>
        /// <param name="errorMessage">錯誤訊息</param>
        /// <param name="pagePath">發生錯誤的頁面路徑</param>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="userAgent">用戶代理</param>
        /// <returns>活動記錄 ID</returns>
        Task<Guid> LogErrorAsync(Guid userId, string adminName, int role, string errorMessage, 
            string pagePath, string ipAddress, string userAgent);

        #endregion

        #region 查詢操作

        /// <summary>
        /// 取得管理員活動記錄（分頁）
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns>分頁活動記錄</returns>
        Task<PagedActivityLogDto> GetActivitiesAsync(ActivityLogFilterDto filter);

        /// <summary>
        /// 取得指定用戶的活動記錄
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>活動記錄列表</returns>
        Task<PagedActivityLogDto> GetUserActivitiesAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得成功的操作記錄
        /// </summary>
        /// <param name="userId">用戶 ID（可選）</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>成功操作記錄</returns>
        Task<PagedActivityLogDto> GetSuccessfulActivitiesAsync(Guid? userId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得失敗的操作記錄
        /// </summary>
        /// <param name="userId">用戶 ID（可選）</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>失敗操作記錄</returns>
        Task<PagedActivityLogDto> GetFailedActivitiesAsync(Guid? userId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得特定操作類型的活動記錄
        /// </summary>
        /// <param name="actionType">操作類型</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>活動記錄列表</returns>
        Task<PagedActivityLogDto> GetActivitiesByTypeAsync(string actionType, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得特定頁面的活動記錄
        /// </summary>
        /// <param name="pagePath">頁面路徑</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>活動記錄列表</returns>
        Task<PagedActivityLogDto> GetActivitiesByPageAsync(string pagePath, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得特定IP地址的活動記錄
        /// </summary>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>活動記錄列表</returns>
        Task<PagedActivityLogDto> GetActivitiesByIpAsync(string ipAddress, int page = 1, int pageSize = 20);

        /// <summary>
        /// 取得時間範圍內的活動記錄
        /// </summary>
        /// <param name="startDate">開始時間</param>
        /// <param name="endDate">結束時間</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>活動記錄列表</returns>
        Task<PagedActivityLogDto> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate, 
            int page = 1, int pageSize = 20);

        #endregion

        #region 統計分析

        /// <summary>
        /// 取得活動統計資料
        /// </summary>
        /// <param name="startDate">開始時間</param>
        /// <param name="endDate">結束時間</param>
        /// <returns>統計資料</returns>
        Task<ActivityLogStatsDto> GetActivityStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得用戶最後成功登入記錄
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <returns>最後成功登入記錄</returns>
        Task<AdminActivityLogDto?> GetLastSuccessfulLoginAsync(Guid userId);

        /// <summary>
        /// 計算用戶失敗操作次數
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="since">統計起始時間</param>
        /// <returns>失敗操作次數</returns>
        Task<int> CountFailedActivitiesAsync(Guid userId, DateTime since);

        /// <summary>
        /// 計算IP地址失敗操作次數
        /// </summary>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="since">統計起始時間</param>
        /// <returns>失敗操作次數</returns>
        Task<int> CountFailedActivitiesByIpAsync(string ipAddress, DateTime since);

        /// <summary>
        /// 取得異常活動記錄（不常用IP等）
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>異常活動記錄</returns>
        Task<PagedActivityLogDto> GetAnomalousActivitiesAsync(Guid userId, int page = 1, int pageSize = 20);

        #endregion

        #region 維護操作

        /// <summary>
        /// 清理過期的活動記錄
        /// </summary>
        /// <param name="expiredBefore">過期時間</param>
        /// <returns>清理的記錄數量</returns>
        Task<int> CleanupExpiredRecordsAsync(DateTime expiredBefore);

        #endregion
    }
}