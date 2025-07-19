using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Reply 實體的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在不同層之間傳輸 Reply 實體的資料，包括：
    /// - 用於 API 回應的資料格式
    /// - 用於前端顯示的回覆資料結構
    /// - 用於服務層之間的資料傳遞
    ///
    /// 注意事項：
    /// - 僅能新增與 Reply 實體相關的屬性
    /// - 包含適當的 Data Annotations 進行驗證
    /// - 此 DTO 主要用於讀取操作，顯示回覆的完整資訊
    /// - 包含計算屬性和輔助方法以增強前端使用體驗
    /// </summary>
    public class ReplyDto
    {
        /// <summary>
        /// 回覆的唯一識別碼
        /// 用途：作為回覆的主要識別
        /// </summary>
        public Guid ReplyId { get; set; }

        /// <summary>
        /// 回覆所屬文章的唯一識別碼
        /// 用途：連結到對應的文章
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 回覆作者的唯一識別碼
        /// 用途：連結到對應的作者
        /// </summary>
        public Guid AuthorId { get; set; }

        /// <summary>
        /// 父回覆的唯一識別碼（用於嵌套回覆）
        /// 用途：建立回覆的階層結構
        /// </summary>
        public Guid? ParentReplyId { get; set; }

        /// <summary>
        /// 回覆的內容文字
        /// 用途：顯示回覆的完整內容
        /// 驗證：必填，最大長度 1000 個字元
        /// </summary>
        [Required(ErrorMessage = "回覆內容為必填欄位")]
        [StringLength(1000, ErrorMessage = "回覆內容長度不能超過 1000 個字元")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 回覆的狀態
        /// 用途：控制回覆的顯示狀態
        /// 值說明：0=正常, 1=隱藏, 2=已刪除
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 回覆的建立時間
        /// 用途：顯示回覆發布時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 回覆獲得的讚數量
        /// 用途：顯示回覆的受歡迎程度
        /// </summary>
        public int PraiseCount { get; set; }

        /// <summary>
        /// 回覆作者的個人資料
        /// 用途：顯示回覆作者的基本資訊
        /// </summary>
        public PersonDto? Author { get; set; }

        /// <summary>
        /// 父回覆的資料（用於嵌套回覆顯示）
        /// 用途：顯示被回覆的內容
        /// </summary>
        public ReplyDto? ParentReply { get; set; }

        /// <summary>
        /// 所屬文章的基本資料
        /// 用途：顯示回覆所屬的文章資訊
        /// </summary>
        public ArticleDto? Article { get; set; }

        /// <summary>
        /// 子回覆列表（嵌套回覆）
        /// 用途：顯示對此回覆的回覆
        /// </summary>
        public List<ReplyDto> ChildReplies { get; set; } = new List<ReplyDto>();

        /// <summary>
        /// 獲取回覆狀態的描述文字
        /// 用途：在前端顯示人類可讀的狀態描述
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "正常",
            1 => "隱藏",
            2 => "已刪除",
            _ => "未知"
        };

        /// <summary>
        /// 判斷回覆是否為正常狀態
        /// 用途：前端快速判斷回覆是否可顯示
        /// </summary>
        public bool IsNormalStatus => Status == 0;

        /// <summary>
        /// 判斷是否為頂層回覆（非嵌套回覆）
        /// 用途：前端判斷回覆的顯示層級
        /// </summary>
        public bool IsTopLevel => ParentReplyId == null;

        /// <summary>
        /// 判斷是否為嵌套回覆
        /// 用途：前端判斷回覆的顯示層級
        /// </summary>
        public bool IsNested => ParentReplyId != null;

        /// <summary>
        /// 獲取回覆的簡短內容
        /// 用途：在通知或摘要中顯示簡短的回覆內容
        /// 邏輯：如果內容超過 50 個字元，則截取前 50 個字元並加上省略號
        /// </summary>
        public string ShortContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "此回覆沒有內容。";
                
                return Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content;
            }
        }

        /// <summary>
        /// 獲取回覆發布時間的友善顯示格式
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
        /// 獲取子回覆的數量
        /// 用途：顯示回覆的子回覆總數
        /// </summary>
        public int ChildReplyCount => ChildReplies.Count;

        /// <summary>
        /// 判斷是否有子回覆
        /// 用途：前端快速判斷是否需要顯示子回覆區塊
        /// </summary>
        public bool HasChildReplies => ChildReplyCount > 0;

        /// <summary>
        /// 獲取回覆的層級深度
        /// 用途：控制前端顯示的縮排層級
        /// </summary>
        public int NestingLevel
        {
            get
            {
                int level = 0;
                var current = this;
                while (current.ParentReply != null)
                {
                    level++;
                    current = current.ParentReply;
                }
                return level;
            }
        }

        /// <summary>
        /// 獲取回覆內容的字數
        /// 用途：顯示回覆的字數統計
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 判斷回覆是否受歡迎
        /// 用途：根據讚數判斷回覆是否受歡迎
        /// 邏輯：讚數超過 5 就算受歡迎
        /// </summary>
        public bool IsPopular => PraiseCount >= 5;

        /// <summary>
        /// 獲取回覆的作者顯示名稱
        /// 用途：顯示回覆作者的名稱
        /// </summary>
        public string AuthorName => Author?.EffectiveDisplayName ?? "未知作者";

        /// <summary>
        /// 獲取回覆的作者頭像
        /// 用途：顯示回覆作者的頭像
        /// </summary>
        public string AuthorAvatar => Author?.EffectiveAvatarUrl ?? "/static/img/default-avatar.png";

        /// <summary>
        /// 獲取被回覆者的顯示名稱（用於嵌套回覆）
        /// 用途：顯示「回覆給誰」的資訊
        /// </summary>
        public string? ParentAuthorName => ParentReply?.AuthorName;

        /// <summary>
        /// 獲取回覆的完整顯示文字（包含被回覆者資訊）
        /// 用途：在前端顯示完整的回覆資訊
        /// </summary>
        public string FullDisplayText
        {
            get
            {
                if (IsTopLevel)
                {
                    return $"{AuthorName}：{Content}";
                }
                else
                {
                    return $"{AuthorName} 回覆 {ParentAuthorName}：{Content}";
                }
            }
        }

        /// <summary>
        /// 判斷回覆是否可以被刪除
        /// 用途：前端權限控制
        /// 邏輯：正常狀態且沒有子回覆才能刪除
        /// </summary>
        public bool CanBeDeleted => IsNormalStatus && !HasChildReplies;

        /// <summary>
        /// 獲取回覆的統計資訊
        /// 用途：顯示回覆的讚數和子回覆數的統計資訊
        /// </summary>
        public string StatsSummary
        {
            get
            {
                var parts = new List<string>();
                
                if (PraiseCount > 0)
                {
                    parts.Add($"{PraiseCount} 個讚");
                }
                
                if (ChildReplyCount > 0)
                {
                    parts.Add($"{ChildReplyCount} 個回覆");
                }
                
                return parts.Count > 0 ? string.Join("，", parts) : "無互動";
            }
        }
    }

    /// <summary>
    /// 用於建立 Reply 的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在建立新回覆時傳輸資料，包括：
    /// - 接收前端提交的新回覆資料
    /// - 進行資料驗證
    /// - 傳遞給服務層處理
    ///
    /// 注意事項：
    /// - 僅能新增與建立 Reply 相關的屬性
    /// - 包含完整的 Data Annotations 進行驗證
    /// - 此 DTO 專門用於建立操作，不包含系統生成的欄位
    /// - 包含適當的驗證規則確保資料完整性
    /// </summary>
    public class CreateReplyDto
    {
        /// <summary>
        /// 回覆所屬文章的唯一識別碼
        /// 用途：指定回覆的目標文章
        /// 驗證：必填，必須是有效的 GUID
        /// </summary>
        [Required(ErrorMessage = "文章 ID 為必填欄位")]
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 父回覆的唯一識別碼（用於嵌套回覆）
        /// 用途：建立回覆的階層結構
        /// 驗證：選填，如果提供必須是有效的 GUID
        /// </summary>
        public Guid? ParentReplyId { get; set; }

        /// <summary>
        /// 回覆的內容文字
        /// 用途：使用者輸入的回覆內容
        /// 驗證：必填，長度限制 1-1000 個字元
        /// </summary>
        [Required(ErrorMessage = "回覆內容為必填欄位")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "回覆內容長度必須介於 1 到 1000 個字元之間")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 判斷是否為頂層回覆（非嵌套回覆）
        /// 用途：前端判斷回覆的類型
        /// </summary>
        public bool IsTopLevel => ParentReplyId == null;

        /// <summary>
        /// 判斷是否為嵌套回覆
        /// 用途：前端判斷回覆的類型
        /// </summary>
        public bool IsNested => ParentReplyId != null;

        /// <summary>
        /// 獲取回覆內容的字數
        /// 用途：顯示回覆的字數統計
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 判斷內容是否為有效長度
        /// 用途：前端即時驗證內容長度
        /// </summary>
        public bool IsValidLength => ContentLength >= 1 && ContentLength <= 1000;

        /// <summary>
        /// 獲取內容長度的狀態訊息
        /// 用途：提供內容長度的即時反饋
        /// </summary>
        public string ContentLengthStatus
        {
            get
            {
                if (ContentLength == 0) return "請輸入回覆內容";
                if (ContentLength > 1000) return $"內容過長，已超過 {ContentLength - 1000} 個字元";
                if (ContentLength < 5) return "內容太短，建議至少 5 個字元";
                return $"內容長度正常（{ContentLength}/1000 字元）";
            }
        }

        /// <summary>
        /// 獲取內容的預覽文字
        /// 用途：在確認頁面顯示回覆內容的預覽
        /// 邏輯：如果內容超過 50 個字元，則截取前 50 個字元並加上省略號
        /// </summary>
        public string PreviewContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "無內容";
                
                return Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content;
            }
        }

        /// <summary>
        /// 獲取回覆類型的描述文字
        /// 用途：顯示回覆的類型說明
        /// </summary>
        public string ReplyTypeText => IsTopLevel ? "對文章的回覆" : "對回覆的回覆";

        /// <summary>
        /// 驗證回覆資料的完整性
        /// 用途：在提交前進行整體驗證
        /// </summary>
        public bool IsValid => ArticleId != Guid.Empty && !string.IsNullOrWhiteSpace(Content) && IsValidLength;

        /// <summary>
        /// 獲取驗證錯誤訊息
        /// 用途：提供詳細的驗證錯誤資訊
        /// </summary>
        public List<string> ValidationErrors
        {
            get
            {
                var errors = new List<string>();
                
                if (ArticleId == Guid.Empty)
                {
                    errors.Add("必須指定回覆的文章");
                }
                
                if (string.IsNullOrWhiteSpace(Content))
                {
                    errors.Add("回覆內容不能為空");
                }
                
                if (!IsValidLength)
                {
                    if (ContentLength > 1000)
                    {
                        errors.Add($"回覆內容過長，超過 {ContentLength - 1000} 個字元");
                    }
                    else if (ContentLength < 1)
                    {
                        errors.Add("回覆內容不能為空");
                    }
                }
                
                return errors;
            }
        }

        /// <summary>
        /// 清理和規範化輸入內容
        /// 用途：在儲存前清理和規範化使用者輸入
        /// </summary>
        public void NormalizeContent()
        {
            if (!string.IsNullOrEmpty(Content))
            {
                // 移除前後空白字元
                Content = Content.Trim();
                
                // 統一換行符號
                Content = Content.Replace("\n", "\r\n");
                
                // 移除多餘的空行（超過兩個連續換行）
                while (Content.Contains("\n\n\n"))
                {
                    Content = Content.Replace("\n\n\n", "\n\n");
                }
            }
        }

        /// <summary>
        /// 驗證父回覆 ID 的有效性
        /// 用途：確保嵌套回覆的父回覆存在
        /// </summary>
        public bool IsValidParentReplyId()
        {
            // 如果是頂層回覆，不需要驗證父回覆 ID
            if (IsTopLevel) return true;
            
            // 如果是嵌套回覆，父回覆 ID 不能為空且不能等於預設值
            return ParentReplyId.HasValue && ParentReplyId.Value != Guid.Empty;
        }

        /// <summary>
        /// 獲取回覆的建立提示訊息
        /// 用途：提供建立回覆的指引
        /// </summary>
        public string CreateHintText
        {
            get
            {
                if (IsTopLevel)
                {
                    return "您正在對文章進行回覆";
                }
                else
                {
                    return "您正在對其他使用者的回覆進行回覆";
                }
            }
        }

        /// <summary>
        /// 判斷回覆是否包含提及（@符號）
        /// 用途：檢查回覆是否提及其他使用者
        /// </summary>
        public bool ContainsMention => !string.IsNullOrEmpty(Content) && Content.Contains("@");

        /// <summary>
        /// 獲取回覆中的提及列表
        /// 用途：提取回覆中提及的使用者名稱
        /// </summary>
        public List<string> GetMentions()
        {
            var mentions = new List<string>();
            
            if (string.IsNullOrEmpty(Content)) return mentions;
            
            var words = Content.Split(new[] { " ", "\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (word.StartsWith("@") && word.Length > 1)
                {
                    var mention = word.Substring(1);
                    if (!mentions.Contains(mention))
                    {
                        mentions.Add(mention);
                    }
                }
            }
            
            return mentions;
        }

        /// <summary>
        /// 將 CreateReplyDto 轉換為建立 Reply 實體所需的基本資料
        /// 用途：提供轉換方法供服務層使用
        /// </summary>
        public Dictionary<string, object> ToCreateData()
        {
            NormalizeContent();
            
            var data = new Dictionary<string, object>
            {
                ["ArticleId"] = ArticleId,
                ["Content"] = Content,
                ["Status"] = 0, // 新回覆預設為正常狀態
                ["CreateTime"] = DateTime.Now,
                ["PraiseCount"] = 0
            };
            
            // 只有在有父回覆時才加入 ParentReplyId
            if (ParentReplyId.HasValue && ParentReplyId.Value != Guid.Empty)
            {
                data["ParentReplyId"] = ParentReplyId.Value;
            }
            
            return data;
        }

        /// <summary>
        /// 驗證建立回覆的完整性
        /// 用途：在提交前進行最終驗證
        /// </summary>
        public (bool IsValid, List<string> Errors) ValidateForCreation()
        {
            var errors = ValidationErrors;
            
            // 額外驗證父回覆 ID
            if (!IsValidParentReplyId())
            {
                errors.Add("嵌套回覆必須指定有效的父回覆");
            }
            
            return (errors.Count == 0, errors);
        }
    }
}
