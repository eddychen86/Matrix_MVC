using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// 管理員活動記錄資料傳輸物件
    /// </summary>
    public class AdminActivityLogDto
    {
        /// <summary>
        /// 活動記錄主鍵 ID
        /// </summary>
        public Guid LoginId { get; set; }

        /// <summary>
        /// 管理員用戶的 UserId
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 管理員用戶名稱
        /// </summary>
        public string AdminName { get; set; } = string.Empty;

        /// <summary>
        /// 管理員角色等級 (1=管理員, 2=超級管理員)
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// 活動發生時的 IP 地址
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 瀏覽器和設備資訊
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// 登入時間 (Session 開始時間)
        /// </summary>
        public DateTime? LoginTime { get; set; }

        /// <summary>
        /// 登出時間 (Session 結束時間)
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// 活動發生時間
        /// </summary>
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// 頁面路徑
        /// </summary>
        public string PagePath { get; set; } = string.Empty;

        /// <summary>
        /// 操作類型 (LOGIN, LOGOUT, VIEW, CREATE, UPDATE, DELETE, ERROR)
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// 活動描述
        /// </summary>
        public string ActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 在頁面停留的時間 (秒)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// 錯誤訊息 (當 IsSuccessful = false 時)
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 創建管理員活動記錄的資料傳輸物件
    /// </summary>
    public class CreateActivityLogDto
    {
        /// <summary>
        /// 管理員用戶的 UserId
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 管理員用戶名稱
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AdminName { get; set; } = string.Empty;

        /// <summary>
        /// 管理員角色等級 (1=管理員, 2=超級管理員)
        /// </summary>
        [Range(1, 2)]
        public int Role { get; set; }

        /// <summary>
        /// 活動發生時的 IP 地址
        /// </summary>
        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 瀏覽器和設備資訊
        /// </summary>
        [Required]
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// 登入時間 (Session 開始時間)
        /// </summary>
        public DateTime? LoginTime { get; set; }

        /// <summary>
        /// 登出時間 (Session 結束時間)
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// 頁面路徑
        /// </summary>
        [StringLength(500)]
        public string PagePath { get; set; } = string.Empty;

        /// <summary>
        /// 操作類型 (LOGIN, LOGOUT, VIEW, CREATE, UPDATE, DELETE, ERROR)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// 活動描述
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string ActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 在頁面停留的時間 (秒)
        /// </summary>
        public int Duration { get; set; } = 0;

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// 錯誤訊息 (當 IsSuccessful = false 時)
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 查詢管理員活動記錄的篩選條件
    /// </summary>
    public class ActivityLogFilterDto
    {
        /// <summary>
        /// 管理員用戶的 UserId
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作類型篩選
        /// </summary>
        public string? ActionType { get; set; }

        /// <summary>
        /// 頁面路徑篩選
        /// </summary>
        public string? PagePath { get; set; }

        /// <summary>
        /// IP 地址篩選
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 是否只查詢成功的操作
        /// </summary>
        public bool? IsSuccessful { get; set; }

        /// <summary>
        /// 頁數
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 管理員活動統計資料
    /// </summary>
    public class ActivityLogStatsDto
    {
        /// <summary>
        /// 總活動數量
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// 成功活動數量
        /// </summary>
        public int SuccessfulActivities { get; set; }

        /// <summary>
        /// 失敗活動數量
        /// </summary>
        public int FailedActivities { get; set; }

        /// <summary>
        /// 唯一用戶數量
        /// </summary>
        public int UniqueUsers { get; set; }

        /// <summary>
        /// 成功率百分比
        /// </summary>
        public double SuccessRate => TotalActivities > 0 ? (SuccessfulActivities * 100.0 / TotalActivities) : 0;
    }

    /// <summary>
    /// 分頁查詢結果
    /// </summary>
    public class PagedActivityLogDto
    {
        /// <summary>
        /// 活動記錄列表
        /// </summary>
        public List<AdminActivityLogDto> Items { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 當前頁數
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; }
    }
}