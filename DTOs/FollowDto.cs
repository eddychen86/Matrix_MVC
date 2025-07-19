namespace Matrix.DTOs
{
    /// <summary>
    /// Follow 實體的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在不同層之間傳輸 Follow 實體的資料，包括：
    /// - 用於 API 回應的資料格式
    /// - 用於前端顯示的追蹤資料結構
    /// - 用於服務層之間的資料傳遞
    ///
    /// 注意事項：
    /// - 僅能新增與 Follow 實體相關的屬性
    /// - 包含適當的 Data Annotations 進行驗證
    /// - 此 DTO 主要用於讀取操作，顯示追蹤關係的資訊
    /// - 包含計算屬性和輔助方法以增強前端使用體驗
    /// </summary>
    public class FollowDto
    {
        /// <summary>
        /// 追蹤關係的唯一識別碼
        /// 用途：作為追蹤關係的主要識別
        /// </summary>
        public Guid FollowId { get; set; }

        /// <summary>
        /// 追蹤者的唯一識別碼
        /// 用途：連結到追蹤者的使用者資料
        /// </summary>
        public Guid FollowerId { get; set; }

        /// <summary>
        /// 被追蹤者的唯一識別碼
        /// 用途：連結到被追蹤者的使用者資料
        /// </summary>
        public Guid FollowedId { get; set; }

        /// <summary>
        /// 追蹤關係的建立時間
        /// 用途：顯示何時開始追蹤
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 追蹤關係的狀態
        /// 用途：控制追蹤關係的顯示和處理狀態
        /// 值說明：0=正常追蹤, 1=已取消追蹤, 2=被封鎖
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 追蹤者的個人資料
        /// 用途：顯示追蹤者的基本資訊
        /// </summary>
        public PersonDto? Follower { get; set; }

        /// <summary>
        /// 被追蹤者的個人資料
        /// 用途：顯示被追蹤者的基本資訊
        /// </summary>
        public PersonDto? Followed { get; set; }

        /// <summary>
        /// 獲取追蹤狀態的描述文字
        /// 用途：在前端顯示人類可讀的狀態描述
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "正常追蹤",
            1 => "已取消追蹤",
            2 => "被封鎖",
            _ => "未知狀態"
        };

        /// <summary>
        /// 判斷追蹤關係是否為正常狀態
        /// 用途：前端快速判斷追蹤關係是否有效
        /// </summary>
        public bool IsActiveFollow => Status == 0;

        /// <summary>
        /// 判斷追蹤關係是否已取消
        /// 用途：前端快速判斷追蹤關係狀態
        /// </summary>
        public bool IsUnfollowed => Status == 1;

        /// <summary>
        /// 判斷追蹤關係是否被封鎖
        /// 用途：前端快速判斷追蹤關係狀態
        /// </summary>
        public bool IsBlocked => Status == 2;

        /// <summary>
        /// 獲取追蹤開始時間的友善顯示格式
        /// 用途：在前端顯示人類可讀的時間格式
        /// </summary>
        public string TimeAgoText
        {
            get
            {
                var timeSpan = DateTime.Now - CreateTime;
                
                return timeSpan.TotalDays switch
                {
                    > 365 => $"{(int)(timeSpan.TotalDays / 365)} 年前開始追蹤",
                    > 30 => $"{(int)(timeSpan.TotalDays / 30)} 個月前開始追蹤",
                    > 7 => $"{(int)(timeSpan.TotalDays / 7)} 週前開始追蹤",
                    > 1 => $"{(int)timeSpan.TotalDays} 天前開始追蹤",
                    _ => timeSpan.TotalHours > 1 ? $"{(int)timeSpan.TotalHours} 小時前開始追蹤" : "剛剛開始追蹤"
                };
            }
        }

        /// <summary>
        /// 獲取追蹤者的顯示名稱
        /// 用途：顯示追蹤者的名稱
        /// </summary>
        public string FollowerName => Follower?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取被追蹤者的顯示名稱
        /// 用途：顯示被追蹤者的名稱
        /// </summary>
        public string FollowedName => Followed?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取追蹤者的頭像
        /// 用途：顯示追蹤者的頭像
        /// </summary>
        public string FollowerAvatar => Follower?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";

        /// <summary>
        /// 獲取被追蹤者的頭像
        /// 用途：顯示被追蹤者的頭像
        /// </summary>
        public string FollowedAvatar => Followed?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";

        /// <summary>
        /// 獲取追蹤關係的描述文字
        /// 用途：在前端顯示完整的追蹤關係描述
        /// </summary>
        public string RelationshipDescription => $"{FollowerName} 追蹤了 {FollowedName}";

        /// <summary>
        /// 判斷追蹤者是否為公開帳戶
        /// 用途：前端判斷是否可以查看追蹤者的資料
        /// </summary>
        public bool IsFollowerPublic => Follower?.IsPublic ?? false;

        /// <summary>
        /// 判斷被追蹤者是否為公開帳戶
        /// 用途：前端判斷是否可以查看被追蹤者的資料
        /// </summary>
        public bool IsFollowedPublic => Followed?.IsPublic ?? false;

        /// <summary>
        /// 判斷是否可以查看追蹤者的完整資料
        /// 用途：前端權限控制
        /// </summary>
        public bool CanViewFollowerProfile => IsFollowerPublic && IsActiveFollow;

        /// <summary>
        /// 判斷是否可以查看被追蹤者的完整資料
        /// 用途：前端權限控制
        /// </summary>
        public bool CanViewFollowedProfile => IsFollowedPublic && IsActiveFollow;

        /// <summary>
        /// 獲取追蹤關係的狀態圖示 CSS 類別
        /// 用途：前端顯示不同狀態的圖示
        /// </summary>
        public string StatusIconClass => Status switch
        {
            0 => "fa-user-check text-success",    // 正常追蹤
            1 => "fa-user-times text-warning",   // 已取消追蹤
            2 => "fa-ban text-danger",           // 被封鎖
            _ => "fa-question text-muted"        // 未知狀態
        };

        /// <summary>
        /// 獲取追蹤關係的優先級
        /// 用途：前端排序追蹤關係的優先級
        /// 邏輯：數字越大優先級越高
        /// </summary>
        public int Priority => Status switch
        {
            0 => 3,  // 正常追蹤 - 最高優先級
            1 => 2,  // 已取消追蹤 - 中優先級
            2 => 1,  // 被封鎖 - 低優先級
            _ => 0   // 未知狀態 - 最低優先級
        };

        /// <summary>
        /// 判斷是否為相互追蹤關係
        /// 用途：前端顯示相互追蹤的特殊標記
        /// 注意：此屬性需要在服務層中設定，因為需要查詢雙向關係
        /// </summary>
        public bool IsMutualFollow { get; set; }

        /// <summary>
        /// 獲取相互追蹤的描述文字
        /// 用途：顯示相互追蹤的狀態
        /// </summary>
        public string MutualFollowText => IsMutualFollow ? "相互追蹤" : "單向追蹤";

        /// <summary>
        /// 判斷追蹤關係是否為今天建立
        /// 用途：前端分組顯示追蹤關係
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷追蹤關係是否為昨天建立
        /// 用途：前端分組顯示追蹤關係
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷追蹤關係是否為本週建立
        /// 用途：前端分組顯示追蹤關係
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);

        /// <summary>
        /// 判斷追蹤關係是否為本月建立
        /// 用途：前端分組顯示追蹤關係
        /// </summary>
        public bool IsThisMonth => CreateTime.Date >= DateTime.Today.AddDays(-30);

        /// <summary>
        /// 獲取追蹤關係的動作按鈕文字
        /// 用途：顯示可執行的動作
        /// </summary>
        public string ActionText => Status switch
        {
            0 => "取消追蹤",
            1 => "重新追蹤",
            2 => "解除封鎖",
            _ => "無法操作"
        };

        /// <summary>
        /// 判斷是否可以執行動作
        /// 用途：前端判斷是否顯示動作按鈕
        /// </summary>
        public bool CanTakeAction => Status != 2; // 被封鎖狀態無法執行動作

        /// <summary>
        /// 獲取追蹤關係的詳細資訊
        /// 用途：在詳情頁面顯示完整的追蹤關係資訊
        /// </summary>
        public Dictionary<string, object> GetDetailInfo()
        {
            return new Dictionary<string, object>
            {
                ["FollowId"] = FollowId,
                ["FollowerName"] = FollowerName,
                ["FollowedName"] = FollowedName,
                ["Status"] = StatusText,
                ["CreateTime"] = CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["TimeAgo"] = TimeAgoText,
                ["IsMutualFollow"] = IsMutualFollow,
                ["IsActiveFollow"] = IsActiveFollow
            };
        }
    }
}
