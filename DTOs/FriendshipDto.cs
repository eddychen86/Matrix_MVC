namespace Matrix.DTOs
{
    /// <summary>
    /// Friendship 實體的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在不同層之間傳輸 Friendship 實體的資料，包括：
    /// - 用於 API 回應的資料格式
    /// - 用於前端顯示的好友關係資料結構
    /// - 用於服務層之間的資料傳遞
    ///
    /// 注意事項：
    /// - 僅能新增與 Friendship 實體相關的屬性
    /// - 包含適當的 Data Annotations 進行驗證
    /// - 此 DTO 主要用於讀取操作，顯示好友關係的資訊
    /// - 包含計算屬性和輔助方法以增強前端使用體驗
    /// </summary>
    public class FriendshipDto
    {
        /// <summary>
        /// 好友關係的唯一識別碼
        /// 用途：作為好友關係的主要識別
        /// </summary>
        public Guid FriendshipId { get; set; }

        /// <summary>
        /// 好友請求發送者的唯一識別碼
        /// 用途：連結到發送好友請求的使用者
        /// </summary>
        public Guid RequesterId { get; set; }

        /// <summary>
        /// 好友請求接收者的唯一識別碼
        /// 用途：連結到接收好友請求的使用者
        /// </summary>
        public Guid AddresseeId { get; set; }

        /// <summary>
        /// 好友關係的狀態
        /// 用途：控制好友關係的處理狀態
        /// 值說明：0=待審核, 1=已接受, 2=已拒絕, 3=已封鎖
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 好友請求的建立時間
        /// 用途：顯示好友請求的發送時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 好友請求的回應時間
        /// 用途：顯示好友請求被接受或拒絕的時間
        /// </summary>
        public DateTime? ResponseTime { get; set; }

        /// <summary>
        /// 好友請求發送者的個人資料
        /// 用途：顯示發送者的基本資訊
        /// </summary>
        public PersonDto? Requester { get; set; }

        /// <summary>
        /// 好友請求接收者的個人資料
        /// 用途：顯示接收者的基本資訊
        /// </summary>
        public PersonDto? Addressee { get; set; }

        /// <summary>
        /// 獲取好友關係狀態的描述文字
        /// 用途：在前端顯示人類可讀的狀態描述
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
        /// 用途：前端快速判斷請求狀態
        /// </summary>
        public bool IsPending => Status == 0;

        /// <summary>
        /// 判斷好友關係是否為已接受狀態
        /// 用途：前端快速判斷好友關係是否有效
        /// </summary>
        public bool IsAccepted => Status == 1;

        /// <summary>
        /// 判斷好友關係是否為已拒絕狀態
        /// 用途：前端快速判斷請求狀態
        /// </summary>
        public bool IsRejected => Status == 2;

        /// <summary>
        /// 判斷好友關係是否為已封鎖狀態
        /// 用途：前端快速判斷請求狀態
        /// </summary>
        public bool IsBlocked => Status == 3;

        /// <summary>
        /// 判斷好友關係是否為有效狀態（已接受的好友關係）
        /// 用途：前端快速判斷是否為真正的好友關係
        /// </summary>
        public bool IsActiveFriendship => IsAccepted;

        /// <summary>
        /// 獲取好友請求發送時間的友善顯示格式
        /// 用途：在前端顯示人類可讀的時間格式
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
        /// 用途：在前端顯示人類可讀的回應時間格式
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
        /// 用途：顯示發送者的名稱
        /// </summary>
        public string RequesterName => Requester?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取好友請求接收者的顯示名稱
        /// 用途：顯示接收者的名稱
        /// </summary>
        public string AddresseeName => Addressee?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取好友請求發送者的頭像
        /// 用途：顯示發送者的頭像
        /// </summary>
        public string RequesterAvatar => Requester?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";

        /// <summary>
        /// 獲取好友請求接收者的頭像
        /// 用途：顯示接收者的頭像
        /// </summary>
        public string AddresseeAvatar => Addressee?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";

        /// <summary>
        /// 獲取好友關係的描述文字
        /// 用途：在前端顯示完整的好友關係描述
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
        /// 用途：前端顯示不同狀態的圖示
        /// </summary>
        public string StatusIconClass => Status switch
        {
            0 => "fa-clock text-warning",          // 待審核
            1 => "fa-handshake text-success",     // 已接受
            2 => "fa-times text-danger",          // 已拒絕
            3 => "fa-ban text-danger",            // 已封鎖
            _ => "fa-question text-muted"         // 未知狀態
        };

        /// <summary>
        /// 獲取好友關係的優先級
        /// 用途：前端排序好友關係的優先級
        /// 邏輯：數字越大優先級越高
        /// </summary>
        public int Priority => Status switch
        {
            0 => 4,  // 待審核 - 最高優先級
            1 => 3,  // 已接受 - 高優先級
            2 => 2,  // 已拒絕 - 中優先級
            3 => 1,  // 已封鎖 - 低優先級
            _ => 0   // 未知狀態 - 最低優先級
        };

        /// <summary>
        /// 判斷好友請求是否需要立即處理
        /// 用途：前端判斷是否需要彈出提醒
        /// </summary>
        public bool RequiresImmediateAttention => IsPending;

        /// <summary>
        /// 判斷發送者是否為公開帳戶
        /// 用途：前端判斷是否可以查看發送者的資料
        /// </summary>
        public bool IsRequesterPublic => Requester?.IsPublic ?? false;

        /// <summary>
        /// 判斷接收者是否為公開帳戶
        /// 用途：前端判斷是否可以查看接收者的資料
        /// </summary>
        public bool IsAddresseePublic => Addressee?.IsPublic ?? false;

        /// <summary>
        /// 判斷是否可以查看發送者的完整資料
        /// 用途：前端權限控制
        /// </summary>
        public bool CanViewRequesterProfile => IsRequesterPublic || IsActiveFriendship;

        /// <summary>
        /// 判斷是否可以查看接收者的完整資料
        /// 用途：前端權限控制
        /// </summary>
        public bool CanViewAddresseeProfile => IsAddresseePublic || IsActiveFriendship;

        /// <summary>
        /// 獲取好友請求的動作按鈕文字
        /// 用途：顯示可執行的動作
        /// </summary>
        public List<string> AvailableActions
        {
            get
            {
                var actions = new List<string>();
                
                switch (Status)
                {
                    case 0: // 待審核
                        actions.Add("接受");
                        actions.Add("拒絕");
                        actions.Add("封鎖");
                        break;
                    case 1: // 已接受
                        actions.Add("刪除好友");
                        actions.Add("封鎖");
                        break;
                    case 2: // 已拒絕
                        actions.Add("重新發送請求");
                        actions.Add("封鎖");
                        break;
                    case 3: // 已封鎖
                        actions.Add("解除封鎖");
                        break;
                }
                
                return actions;
            }
        }

        /// <summary>
        /// 判斷是否可以執行動作
        /// 用途：前端判斷是否顯示動作按鈕
        /// </summary>
        public bool CanTakeAction => Status != 3; // 封鎖狀態也可以執行解除封鎖動作

        /// <summary>
        /// 獲取好友關係的成立時間
        /// 用途：顯示好友關係的建立時間
        /// </summary>
        public DateTime? FriendshipEstablishedTime => IsAccepted ? ResponseTime : null;

        /// <summary>
        /// 獲取好友關係的持續時間
        /// 用途：顯示好友關係已持續多長時間
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
        /// 用途：前端分組顯示好友關係
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷好友關係是否為昨天建立
        /// 用途：前端分組顯示好友關係
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷好友關係是否為本週建立
        /// 用途：前端分組顯示好友關係
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);

        /// <summary>
        /// 判斷好友關係是否為本月建立
        /// 用途：前端分組顯示好友關係
        /// </summary>
        public bool IsThisMonth => CreateTime.Date >= DateTime.Today.AddDays(-30);

        /// <summary>
        /// 獲取好友關係的互動統計資訊
        /// 用途：顯示好友之間的互動情況
        /// 注意：此屬性需要在服務層中設定，因為需要查詢相關的互動資料
        /// </summary>
        public Dictionary<string, int> InteractionStats { get; set; } = [];

        /// <summary>
        /// 判斷是否為活躍的好友關係
        /// 用途：根據互動統計判斷好友關係是否活躍
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
        /// 用途：在詳情頁面顯示完整的好友關係資訊
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
        /// 用途：在好友列表中根據當前使用者獲取對方的資訊
        /// </summary>
        public PersonDto? GetOtherPerson(Guid currentUserId)
        {
            if (RequesterId == currentUserId) return Addressee;
            if (AddresseeId == currentUserId) return Requester;
            return null;
        }

        /// <summary>
        /// 根據使用者 ID 獲取對方的名稱
        /// 用途：在好友列表中顯示對方的名稱
        /// </summary>
        public string GetOtherPersonName(Guid currentUserId)
        {
            var otherPerson = GetOtherPerson(currentUserId);
            return otherPerson?.EffectiveDisplayName ?? "未知使用者";
        }

        /// <summary>
        /// 根據使用者 ID 獲取對方的頭像
        /// 用途：在好友列表中顯示對方的頭像
        /// </summary>
        public string GetOtherPersonAvatar(Guid currentUserId)
        {
            var otherPerson = GetOtherPerson(currentUserId);
            return otherPerson?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";
        }

        /// <summary>
        /// 判斷當前使用者是否為好友請求的發送者
        /// 用途：前端權限控制和UI顯示
        /// </summary>
        public bool IsRequester(Guid currentUserId) => RequesterId == currentUserId;

        /// <summary>
        /// 判斷當前使用者是否為好友請求的接收者
        /// 用途：前端權限控制和UI顯示
        /// </summary>
        public bool IsAddressee(Guid currentUserId) => AddresseeId == currentUserId;
    }
}
