using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Notification 實體的資料傳輸物件
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// 通知的唯一識別碼
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// 通知接收者的唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 通知發送者的唯一識別碼
        /// </summary>
        public Guid? SenderId { get; set; }

        /// <summary>
        /// 通知的類型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 通知的標題
        /// </summary>
        [Required(ErrorMessage = "通知標題為必填欄位")]
        [StringLength(100, ErrorMessage = "通知標題長度不能超過 100 個字元")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知的內容
        /// </summary>
        [StringLength(500, ErrorMessage = "通知內容長度不能超過 500 個字元")]
        public string? Content { get; set; }

        /// <summary>
        /// 通知的狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 通知的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 通知的讀取時間
        /// </summary>
        public DateTime? ReadTime { get; set; }

        /// <summary>
        /// 相關的資料 ID
        /// </summary>
        public Guid? RelatedId { get; set; }

        /// <summary>
        /// 通知接收者的個人資料
        /// </summary>
        public PersonDto? User { get; set; }

        /// <summary>
        /// 通知發送者的個人資料
        /// </summary>
        public PersonDto? Sender { get; set; }

        /// <summary>
        /// 獲取通知類型的描述文字
        /// </summary>
        public string TypeText => Type switch
        {
            0 => "系統通知",
            1 => "讚通知",
            2 => "回覆通知",
            3 => "追蹤通知",
            4 => "好友請求",
            5 => "其他",
            _ => "未知類型"
        };

        /// <summary>
        /// 獲取通知狀態的描述文字
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "未讀",
            1 => "已讀",
            2 => "已刪除",
            _ => "未知狀態"
        };

        /// <summary>
        /// 判斷通知是否為未讀狀態
        /// </summary>
        public bool IsUnread => Status == 0;

        /// <summary>
        /// 判斷通知是否為已讀狀態
        /// </summary>
        public bool IsRead => Status == 1;

        /// <summary>
        /// 判斷通知是否為已刪除狀態
        /// </summary>
        public bool IsDeleted => Status == 2;

        /// <summary>
        /// 判斷通知是否為系統通知
        /// </summary>
        public bool IsSystemNotification => Type == 0 || SenderId == null;

        /// <summary>
        /// 判斷通知是否為使用者通知
        /// </summary>
        public bool IsUserNotification => !IsSystemNotification;

        /// <summary>
        /// 獲取通知發送時間的友善顯示格式
        /// </summary>
        public string TimeAgoText
        {
            get
            {
                var timeSpan = DateTime.Now - CreateTime;
                
                return timeSpan.TotalDays switch
                {
                    > 365 => $"{(int)(timeSpan.TotalDays / 365)} 年前",
                    > 30 => $"{(int)(timeSpan.TotalDays / 30)} 個月前",
                    > 7 => $"{(int)(timeSpan.TotalDays / 7)} 週前",
                    > 1 => $"{(int)timeSpan.TotalDays} 天前",
                    _ => timeSpan.TotalHours > 1 ? $"{(int)timeSpan.TotalHours} 小時前" : "剛剛"
                };
            }
        }

        /// <summary>
        /// 獲取通知的簡短內容
        /// </summary>
        public string ShortContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return Title;
                
                return Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content;
            }
        }

        /// <summary>
        /// 獲取通知的發送者名稱
        /// </summary>
        public string SenderName => Sender?.EffectiveDisplayName ?? "系統";

        /// <summary>
        /// 獲取通知的發送者頭像
        /// </summary>
        public string SenderAvatar => !string.IsNullOrEmpty(Sender?.AvatarPath) ? Sender.AvatarPath : "/static/img/system-avatar.png";

        /// <summary>
        /// 獲取通知的接收者名稱
        /// </summary>
        public string ReceiverName => User?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取通知的圖示 CSS 類別
        /// </summary>
        public string IconClass => Type switch
        {
            0 => "fa-info-circle text-info",
            1 => "fa-thumbs-up text-success",
            2 => "fa-reply text-primary",
            3 => "fa-user-plus text-warning",
            4 => "fa-handshake text-secondary",
            5 => "fa-bell text-muted",
            _ => "fa-question text-muted"
        };

        /// <summary>
        /// 獲取通知的優先級
        /// </summary>
        public int Priority => Type switch
        {
            0 => 5,
            4 => 4,
            3 => 3,
            2 => 2,
            1 => 1,
            _ => 0
        };

        /// <summary>
        /// 判斷通知是否需要立即處理
        /// </summary>
        public bool RequiresImmediateAttention => Type == 0 || Type == 4;

        /// <summary>
        /// 獲取通知的完整顯示文字
        /// </summary>
        public string FullDisplayText
        {
            get
            {
                if (IsSystemNotification)
                {
                    return $"系統通知：{Title}";
                }
                else
                {
                    return $"{SenderName}：{Title}";
                }
            }
        }

        /// <summary>
        /// 獲取通知的動作按鈕文字
        /// </summary>
        public string ActionText => Type switch
        {
            1 => "查看",
            2 => "回覆",
            3 => "查看個人檔案",
            4 => "處理請求",
            _ => "查看詳情"
        };

        /// <summary>
        /// 判斷通知是否可以執行動作
        /// </summary>
        public bool CanTakeAction => !IsDeleted && RelatedId.HasValue;

        /// <summary>
        /// 獲取通知的動作連結
        /// </summary>
        public string ActionUrl
        {
            get
            {
                if (!RelatedId.HasValue) return "#";
                
                return Type switch
                {
                    1 => $"/articles/{RelatedId}",
                    2 => $"/articles/{RelatedId}#replies",
                    3 => $"/users/{SenderId}",
                    4 => "/friends/requests",
                    _ => "#"
                };
            }
        }

        /// <summary>
        /// 獲取通知的讀取狀態文字
        /// </summary>
        public string ReadStatusText
        {
            get
            {
                if (IsUnread) return "未讀";
                if (ReadTime.HasValue)
                {
                    var timeSpan = DateTime.Now - ReadTime.Value;
                    return timeSpan.TotalDays switch
                    {
                        > 1 => $"已讀 - {(int)timeSpan.TotalDays} 天前",
                        _ => timeSpan.TotalHours > 1 ? $"已讀 - {(int)timeSpan.TotalHours} 小時前" : "已讀 - 剛剛"
                    };
                }
                return "已讀";
            }
        }

        /// <summary>
        /// 判斷通知是否為今天的通知
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷通知是否為昨天的通知
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷通知是否為本週的通知
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);
    }
}