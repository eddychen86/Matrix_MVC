namespace Matrix.DTOs
{
    /// <summary>
    /// Follow 實體的資料傳輸物件
    /// </summary>
    public class FollowDto
    {
        /// <summary>
        /// 追蹤關係的唯一識別碼
        /// </summary>
        public Guid FollowId { get; set; }

        /// <summary>
        /// 追蹤者的唯一識別碼
        /// </summary>
        public Guid FollowerId { get; set; }

        /// <summary>
        /// 被追蹤者的唯一識別碼
        /// </summary>
        public Guid FollowedId { get; set; }

        /// <summary>
        /// 追蹤關係的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 追蹤關係的狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 追蹤者的個人資料
        /// </summary>
        public PersonDto? Follower { get; set; }

        /// <summary>
        /// 被追蹤者的個人資料
        /// </summary>
        public PersonDto? Followed { get; set; }

        /// <summary>
        /// 獲取追蹤狀態的描述文字
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
        /// </summary>
        public bool IsActiveFollow => Status == 0;

        /// <summary>
        /// 判斷追蹤關係是否已取消
        /// </summary>
        public bool IsUnfollowed => Status == 1;

        /// <summary>
        /// 判斷追蹤關係是否被封鎖
        /// </summary>
        public bool IsBlocked => Status == 2;


        /// <summary>
        /// 獲取追蹤者的顯示名稱
        /// </summary>
        public string FollowerName => Follower?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取被追蹤者的顯示名稱
        /// </summary>
        public string FollowedName => Followed?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取追蹤者的頭像
        /// </summary>
        public string? FollowerAvatar => !string.IsNullOrEmpty(Follower?.AvatarPath) ? Follower.AvatarPath : null;

        /// <summary>
        /// 獲取被追蹤者的頭像
        /// </summary>
        public string? FollowedAvatar => !string.IsNullOrEmpty(Followed?.AvatarPath) ? Followed.AvatarPath : null;

        /// <summary>
        /// 獲取追蹤關係的描述文字
        /// </summary>
        public string RelationshipDescription => $"{FollowerName} 追蹤了 {FollowedName}";

        /// <summary>
        /// 判斷追蹤者是否為公開帳戶
        /// </summary>
        public bool IsFollowerPublic => Follower?.IsPublic ?? false;

        /// <summary>
        /// 判斷被追蹤者是否為公開帳戶
        /// </summary>
        public bool IsFollowedPublic => Followed?.IsPublic ?? false;

        /// <summary>
        /// 判斷是否可以查看追蹤者的完整資料
        /// </summary>
        public bool CanViewFollowerProfile => IsFollowerPublic && IsActiveFollow;

        /// <summary>
        /// 判斷是否可以查看被追蹤者的完整資料
        /// </summary>
        public bool CanViewFollowedProfile => IsFollowedPublic && IsActiveFollow;

        /// <summary>
        /// 獲取追蹤關係的狀態圖示 CSS 類別
        /// </summary>
        public string StatusIconClass => Status switch
        {
            0 => "fa-user-check text-success",
            1 => "fa-user-times text-warning",
            2 => "fa-ban text-danger",
            _ => "fa-question text-muted"
        };

        /// <summary>
        /// 獲取追蹤關係的優先級
        /// </summary>
        public int Priority => Status switch
        {
            0 => 3,
            1 => 2,
            2 => 1,
            _ => 0
        };

        /// <summary>
        /// 判斷是否為相互追蹤關係
        /// </summary>
        public bool IsMutualFollow { get; set; }

        /// <summary>
        /// 獲取相互追蹤的描述文字
        /// </summary>
        public string MutualFollowText => IsMutualFollow ? "相互追蹤" : "單向追蹤";

        /// <summary>
        /// 判斷追蹤關係是否為今天建立
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷追蹤關係是否為昨天建立
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷追蹤關係是否為本週建立
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);

        /// <summary>
        /// 判斷追蹤關係是否為本月建立
        /// </summary>
        public bool IsThisMonth => CreateTime.Date >= DateTime.Today.AddDays(-30);

        /// <summary>
        /// 獲取追蹤關係的動作按鈕文字
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
        /// </summary>
        public bool CanTakeAction => Status != 2;

        /// <summary>
        /// 獲取追蹤關係的詳細資訊
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
                ["IsMutualFollow"] = IsMutualFollow,
                ["IsActiveFollow"] = IsActiveFollow
            };
        }

    }
    /// <summary>追蹤統計：Followers=被追蹤數、Following=追蹤數</summary>
    public sealed record FollowStatsDto(int Followers, int Following);
}