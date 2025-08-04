namespace Matrix.DTOs
{
    /// <summary>
    /// Friendship 實體的資料傳輸物件
    /// </summary>
    public class FriendshipDto
    {
        /// <summary>
        /// 好友關係的唯一識別碼
        /// </summary>
        public Guid FriendshipId { get; set; }

        /// <summary>
        /// 好友請求發送者的唯一識別碼
        /// </summary>
        public Guid RequesterId { get; set; }

        /// <summary>
        /// 好友請求接收者的唯一識別碼
        /// </summary>
        public Guid AddresseeId { get; set; }

        /// <summary>
        /// 好友關係的狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 好友請求的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 好友請求的回應時間
        /// </summary>
        public DateTime? ResponseTime { get; set; }

        /// <summary>
        /// 好友請求發送者的個人資料
        /// </summary>
        public PersonDto? Requester { get; set; }

        /// <summary>
        /// 好友請求接收者的個人資料
        /// </summary>
        public PersonDto? Addressee { get; set; }

        /// <summary>
        /// 獲取好友關係狀態的描述文字
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "待審核",
            1 => "已接受",
            2 => "已拒絕",
            3 => "已封鎖",
            _ => "未知狀態"
        };

        /// <summary>
        /// 判斷好友關係是否為待審核狀態
        /// </summary>
        public bool IsPending => Status == 0;

        /// <summary>
        /// 判斷好友關係是否為已接受狀態
        /// </summary>
        public bool IsAccepted => Status == 1;

        /// <summary>
        /// 判斷好友關係是否為已拒絕狀態
        /// </summary>
        public bool IsRejected => Status == 2;

        /// <summary>
        /// 判斷好友關係是否為已封鎖狀態
        /// </summary>
        public bool IsBlocked => Status == 3;

        /// <summary>
        /// 判斷好友關係是否為有效狀態
        /// </summary>
        public bool IsActiveFriendship => IsAccepted;

        /// <summary>
        /// 獲取好友請求發送時間的友善顯示格式
        /// </summary>
        public string RequestTimeAgoText
        {
            get
            {
                var timeSpan = DateTime.Now - CreateTime;

                return timeSpan.TotalDays switch
                {
                    > 365 => $"{(int)(timeSpan.TotalDays / 365)} 年前發送",
                    > 30 => $"{(int)(timeSpan.TotalDays / 30)} 個月前發送",
                    > 7 => $"{(int)(timeSpan.TotalDays / 7)} 週前發送",
                    > 1 => $"{(int)timeSpan.TotalDays} 天前發送",
                    _ => timeSpan.TotalHours > 1 ? $"{(int)timeSpan.TotalHours} 小時前發送" : "剛剛發送"
                };
            }
        }

        /// <summary>
        /// 獲取好友請求回應時間的友善顯示格式
        /// </summary>
        public string ResponseTimeAgoText
        {
            get
            {
                if (ResponseTime == null) return "尚未回應";

                var timeSpan = DateTime.Now - ResponseTime.Value;

                return timeSpan.TotalDays switch
                {
                    > 365 => $"{(int)(timeSpan.TotalDays / 365)} 年前回應",
                    > 30 => $"{(int)(timeSpan.TotalDays / 30)} 個月前回應",
                    > 7 => $"{(int)(timeSpan.TotalDays / 7)} 週前回應",
                    > 1 => $"{(int)timeSpan.TotalDays} 天前回應",
                    _ => timeSpan.TotalHours > 1 ? $"{(int)timeSpan.TotalHours} 小時前回應" : "剛剛回應"
                };
            }
        }

        /// <summary>
        /// 獲取好友請求發送者的顯示名稱
        /// </summary>
        public string RequesterName => Requester?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取好友請求接收者的顯示名稱
        /// </summary>
        public string AddresseeName => Addressee?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取好友請求發送者的頭像
        /// </summary>
        public string RequesterAvatar => !string.IsNullOrEmpty(Requester?.AvatarPath) ? Requester.AvatarPath : "/static/images/default_avatar.png";

        /// <summary>
        /// 獲取好友請求接收者的頭像
        /// </summary>
        public string AddresseeAvatar => !string.IsNullOrEmpty(Addressee?.AvatarPath) ? Addressee.AvatarPath : "/static/images/default_avatar.png";

        /// <summary>
        /// 獲取好友關係的描述文字
        /// </summary>
        public string RelationshipDescription => Status switch
        {
            0 => $"{RequesterName} 向 {AddresseeName} 發送了好友請求",
            1 => $"{RequesterName} 和 {AddresseeName} 已經成為好友",
            2 => $"{RequesterName} 的好友請求被 {AddresseeName} 拒絕",
            3 => $"{RequesterName} 被 {AddresseeName} 封鎖",
            _ => $"{RequesterName} 和 {AddresseeName} 的關係狀態未知"
        };

        /// <summary>
        /// 獲取好友關係的狀態圖示 CSS 類別
        /// </summary>
        public string StatusIconClass => Status switch
        {
            0 => "fa-clock text-warning",
            1 => "fa-handshake text-success",
            2 => "fa-times text-danger",
            3 => "fa-ban text-danger",
            _ => "fa-question text-muted"
        };

        /// <summary>
        /// 獲取好友關係的優先級
        /// </summary>
        public int Priority => Status switch
        {
            0 => 4,
            1 => 3,
            2 => 2,
            3 => 1,
            _ => 0
        };

        /// <summary>
        /// 判斷好友請求是否需要立即處理
        /// </summary>
        public bool RequiresImmediateAttention => IsPending;

        /// <summary>
        /// 判斷發送者是否為公開帳戶
        /// </summary>
        public bool IsRequesterPublic => Requester?.IsPublic ?? false;

        /// <summary>
        /// 判斷接收者是否為公開帳戶
        /// </summary>
        public bool IsAddresseePublic => Addressee?.IsPublic ?? false;

        /// <summary>
        /// 判斷是否可以查看發送者的完整資料
        /// </summary>
        public bool CanViewRequesterProfile => IsRequesterPublic || IsActiveFriendship;

        /// <summary>
        /// 判斷是否可以查看接收者的完整資料
        /// </summary>
        public bool CanViewAddresseeProfile => IsAddresseePublic || IsActiveFriendship;

        /// <summary>
        /// 獲取好友請求的動作按鈕文字
        /// </summary>
        public List<string> AvailableActions
        {
            get
            {
                var actions = new List<string>();

                switch (Status)
                {
                    case 0:
                        actions.Add("接受");
                        actions.Add("拒絕");
                        actions.Add("封鎖");
                        break;
                    case 1:
                        actions.Add("刪除好友");
                        actions.Add("封鎖");
                        break;
                    case 2:
                        actions.Add("重新發送請求");
                        actions.Add("封鎖");
                        break;
                    case 3:
                        actions.Add("解除封鎖");
                        break;
                }

                return actions;
            }
        }

        /// <summary>
        /// 判斷是否可以執行動作
        /// </summary>
        public bool CanTakeAction => Status != 3;

        /// <summary>
        /// 獲取好友關係的成立時間
        /// </summary>
        public DateTime? FriendshipEstablishedTime => IsAccepted ? ResponseTime : null;

        /// <summary>
        /// 獲取好友關係的持續時間
        /// </summary>
        public string FriendshipDurationText
        {
            get
            {
                if (!IsAccepted || ResponseTime == null) return "尚未建立好友關係";

                var timeSpan = DateTime.Now - ResponseTime.Value;

                return timeSpan.TotalDays switch
                {
                    > 365 => $"已是好友 {(int)(timeSpan.TotalDays / 365)} 年",
                    > 30 => $"已是好友 {(int)(timeSpan.TotalDays / 30)} 個月",
                    > 7 => $"已是好友 {(int)(timeSpan.TotalDays / 7)} 週",
                    > 1 => $"已是好友 {(int)timeSpan.TotalDays} 天",
                    _ => timeSpan.TotalHours > 1 ? $"已是好友 {(int)timeSpan.TotalHours} 小時" : "剛成為好友"
                };
            }
        }

        /// <summary>
        /// 判斷好友關係是否為今天建立
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷好友關係是否為昨天建立
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷好友關係是否為本週建立
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);

        /// <summary>
        /// 判斷好友關係是否為本月建立
        /// </summary>
        public bool IsThisMonth => CreateTime.Date >= DateTime.Today.AddDays(-30);

        /// <summary>
        /// 獲取好友關係的互動統計資訊
        /// </summary>
        public Dictionary<string, int> InteractionStats { get; set; } = [];

        /// <summary>
        /// 判斷是否為活躍的好友關係
        /// </summary>
        public bool IsActiveFriendshipInteraction
        {
            get
            {
                if (!IsAccepted) return false;

                return InteractionStats.Values.Sum() > 0;
            }
        }

        /// <summary>
        /// 獲取好友關係的詳細資訊
        /// </summary>
        public Dictionary<string, object> GetDetailInfo()
        {
            var info = new Dictionary<string, object>
            {
                ["FriendshipId"] = FriendshipId,
                ["RequesterName"] = RequesterName,
                ["AddresseeName"] = AddresseeName,
                ["Status"] = StatusText,
                ["CreateTime"] = CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["RequestTimeAgo"] = RequestTimeAgoText,
                ["IsActiveFriendship"] = IsActiveFriendship,
                ["RequiresImmediateAttention"] = RequiresImmediateAttention
            };

            if (ResponseTime.HasValue)
            {
                info["ResponseTime"] = ResponseTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                info["ResponseTimeAgo"] = ResponseTimeAgoText;
            }

            if (IsAccepted)
            {
                info["FriendshipDuration"] = FriendshipDurationText;
            }

            return info;
        }

        /// <summary>
        /// 根據使用者 ID 獲取對方的資訊
        /// </summary>
        public PersonDto? GetOtherPerson(Guid currentUserId)
        {
            if (RequesterId == currentUserId) return Addressee;
            if (AddresseeId == currentUserId) return Requester;
            return null;
        }

        /// <summary>
        /// 根據使用者 ID 獲取對方的名稱
        /// </summary>
        public string GetOtherPersonName(Guid currentUserId)
        {
            var otherPerson = GetOtherPerson(currentUserId);
            return otherPerson?.EffectiveDisplayName ?? "未知使用者";
        }

        /// <summary>
        /// 根據使用者 ID 獲取對方的頭像
        /// </summary>
        public string GetOtherPersonAvatar(Guid currentUserId)
        {
            var otherPerson = GetOtherPerson(currentUserId);
            return !string.IsNullOrEmpty(otherPerson?.AvatarPath) ? otherPerson.AvatarPath : "/static/images/default_avatar.png";
        }

        /// <summary>
        /// 判斷當前使用者是否為好友請求的發送者
        /// </summary>
        public bool IsRequester(Guid currentUserId) => RequesterId == currentUserId;

        /// <summary>
        /// 判斷當前使用者是否為好友請求的接收者
        /// </summary>
        public bool IsAddressee(Guid currentUserId) => AddresseeId == currentUserId;
    }
}