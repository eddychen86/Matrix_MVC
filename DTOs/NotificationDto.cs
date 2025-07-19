using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Notification 實體的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在不同層之間傳輸 Notification 實體的資料，包括：
    /// - 用於 API 回應的資料格式
    /// - 用於前端顯示的通知資料結構
    /// - 用於服務層之間的資料傳遞
    ///
    /// 注意事項：
    /// - 僅能新增與 Notification 實體相關的屬性
    /// - 包含適當的 Data Annotations 進行驗證
    /// - 此 DTO 主要用於讀取操作，顯示通知的完整資訊
    /// - 包含計算屬性和輔助方法以增強前端使用體驗
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// 通知的唯一識別碼
        /// 用途：作為通知的主要識別
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// 通知接收者的唯一識別碼
        /// 用途：連結到對應的接收者
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 通知發送者的唯一識別碼
        /// 用途：連結到對應的發送者
        /// </summary>
        public Guid? SenderId { get; set; }

        /// <summary>
        /// 通知的類型
        /// 用途：區分不同種類的通知
        /// 值說明：0=系統通知, 1=讚通知, 2=回覆通知, 3=追蹤通知, 4=好友請求, 5=其他
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 通知的標題
        /// 用途：顯示通知的簡短標題
        /// 驗證：必填，最大長度 100 個字元
        /// </summary>
        [Required(ErrorMessage = "通知標題為必填欄位")]
        [StringLength(100, ErrorMessage = "通知標題長度不能超過 100 個字元")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知的內容
        /// 用途：顯示通知的詳細內容
        /// 驗證：選填，最大長度 500 個字元
        /// </summary>
        [StringLength(500, ErrorMessage = "通知內容長度不能超過 500 個字元")]
        public string? Content { get; set; }

        /// <summary>
        /// 通知的狀態
        /// 用途：控制通知的顯示和處理狀態
        /// 值說明：0=未讀, 1=已讀, 2=已刪除
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 通知的建立時間
        /// 用途：顯示通知發送時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 通知的讀取時間
        /// 用途：記錄使用者讀取通知的時間
        /// </summary>
        public DateTime? ReadTime { get; set; }

        /// <summary>
        /// 相關的資料 ID（如文章 ID、回覆 ID 等）
        /// 用途：連結到通知相關的具體資料
        /// </summary>
        public Guid? RelatedId { get; set; }

        /// <summary>
        /// 通知接收者的個人資料
        /// 用途：顯示接收者的基本資訊
        /// </summary>
        public PersonDto? User { get; set; }

        /// <summary>
        /// 通知發送者的個人資料
        /// 用途：顯示發送者的基本資訊
        /// </summary>
        public PersonDto? Sender { get; set; }

        /// <summary>
        /// 獲取通知類型的描述文字
        /// 用途：在前端顯示人類可讀的通知類型
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
        /// 用途：在前端顯示人類可讀的狀態描述
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
        /// 用途：前端快速判斷通知狀態
        /// </summary>
        public bool IsUnread => Status == 0;

        /// <summary>
        /// 判斷通知是否為已讀狀態
        /// 用途：前端快速判斷通知狀態
        /// </summary>
        public bool IsRead => Status == 1;

        /// <summary>
        /// 判斷通知是否為已刪除狀態
        /// 用途：前端快速判斷通知狀態
        /// </summary>
        public bool IsDeleted => Status == 2;

        /// <summary>
        /// 判斷通知是否為系統通知
        /// 用途：前端區分系統通知和使用者通知
        /// </summary>
        public bool IsSystemNotification => Type == 0 || SenderId == null;

        /// <summary>
        /// 判斷通知是否為使用者通知
        /// 用途：前端區分系統通知和使用者通知
        /// </summary>
        public bool IsUserNotification => !IsSystemNotification;

        /// <summary>
        /// 獲取通知發送時間的友善顯示格式
        /// 用途：在前端顯示人類可讀的時間格式
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
        /// 用途：在通知列表中顯示簡短的通知內容
        /// 邏輯：如果內容超過 50 個字元，則截取前 50 個字元並加上省略號
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
        /// 用途：顯示通知發送者的名稱
        /// </summary>
        public string SenderName => Sender?.EffectiveDisplayName ?? "系統";

        /// <summary>
        /// 獲取通知的發送者頭像
        /// 用途：顯示通知發送者的頭像
        /// </summary>
        public string SenderAvatar => Sender?.EffectiveAvatarUrl ?? "/static/img/system-avatar.png";

        /// <summary>
        /// 獲取通知的接收者名稱
        /// 用途：顯示通知接收者的名稱
        /// </summary>
        public string ReceiverName => User?.EffectiveDisplayName ?? "未知使用者";

        /// <summary>
        /// 獲取通知的圖示 CSS 類別
        /// 用途：前端顯示不同類型通知的圖示
        /// </summary>
        public string IconClass => Type switch
        {
            0 => "fa-info-circle text-info",      // 系統通知
            1 => "fa-thumbs-up text-success",    // 讚通知
            2 => "fa-reply text-primary",        // 回覆通知
            3 => "fa-user-plus text-warning",    // 追蹤通知
            4 => "fa-handshake text-secondary",  // 好友請求
            5 => "fa-bell text-muted",           // 其他
            _ => "fa-question text-muted"        // 未知類型
        };

        /// <summary>
        /// 獲取通知的優先級
        /// 用途：前端排序通知的優先級
        /// 邏輯：數字越大優先級越高
        /// </summary>
        public int Priority => Type switch
        {
            0 => 5,  // 系統通知 - 最高優先級
            4 => 4,  // 好友請求 - 高優先級
            3 => 3,  // 追蹤通知 - 中高優先級
            2 => 2,  // 回覆通知 - 中優先級
            1 => 1,  // 讚通知 - 低優先級
            _ => 0   // 其他 - 最低優先級
        };

        /// <summary>
        /// 判斷通知是否需要立即處理
        /// 用途：前端判斷是否需要彈出提醒
        /// </summary>
        public bool RequiresImmediateAttention => Type == 0 || Type == 4; // 系統通知或好友請求

        /// <summary>
        /// 獲取通知的完整顯示文字
        /// 用途：在通知詳情頁面顯示完整的通知資訊
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
        /// 用途：顯示通知可執行的動作
        /// </summary>
        public string ActionText => Type switch
        {
            1 => "查看",        // 讚通知
            2 => "回覆",        // 回覆通知
            3 => "查看個人檔案", // 追蹤通知
            4 => "處理請求",     // 好友請求
            _ => "查看詳情"      // 其他
        };

        /// <summary>
        /// 判斷通知是否可以執行動作
        /// 用途：前端判斷是否顯示動作按鈕
        /// </summary>
        public bool CanTakeAction => !IsDeleted && RelatedId.HasValue;

        /// <summary>
        /// 獲取通知的動作連結
        /// 用途：提供通知的相關連結
        /// </summary>
        public string ActionUrl
        {
            get
            {
                if (!RelatedId.HasValue) return "#";
                
                return Type switch
                {
                    1 => $"/articles/{RelatedId}",           // 讚通知 - 跳轉到文章
                    2 => $"/articles/{RelatedId}#replies",   // 回覆通知 - 跳轉到文章回覆區
                    3 => $"/users/{SenderId}",               // 追蹤通知 - 跳轉到使用者頁面
                    4 => "/friends/requests",                // 好友請求 - 跳轉到好友請求頁面
                    _ => "#"                                 // 其他 - 無連結
                };
            }
        }

        /// <summary>
        /// 獲取通知的讀取狀態文字
        /// 用途：顯示通知的讀取時間資訊
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
        /// 用途：前端分組顯示通知
        /// </summary>
        public bool IsToday => CreateTime.Date == DateTime.Today;

        /// <summary>
        /// 判斷通知是否為昨天的通知
        /// 用途：前端分組顯示通知
        /// </summary>
        public bool IsYesterday => CreateTime.Date == DateTime.Today.AddDays(-1);

        /// <summary>
        /// 判斷通知是否為本週的通知
        /// 用途：前端分組顯示通知
        /// </summary>
        public bool IsThisWeek => CreateTime.Date >= DateTime.Today.AddDays(-7);
    }
}
