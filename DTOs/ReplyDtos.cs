using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Reply 實體的資料傳輸物件
    /// </summary>
    public class ReplyDto
    {
        /// <summary>
        /// 回覆的唯一識別碼
        /// </summary>
        public Guid ReplyId { get; set; }

        /// <summary>
        /// 回覆所屬文章的唯一識別碼
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 回覆作者的唯一識別碼
        /// </summary>
        public Guid AuthorId { get; set; }

        /// <summary>
        /// 父回覆的唯一識別碼
        /// </summary>
        public Guid? ParentReplyId { get; set; }

        /// <summary>
        /// 回覆的內容文字
        /// </summary>
        [Required(ErrorMessage = "Reply_ContentRequired")]
        [StringLength(1000, ErrorMessage = "Reply_ContentMaxLength1000")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 回覆的狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 回覆的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 回覆獲得的讚數量
        /// </summary>
        public int PraiseCount { get; set; }

        /// <summary>
        /// 回覆作者的個人資料
        /// </summary>
        public PersonDto? Author { get; set; }

        /// <summary>
        /// 父回覆的資料
        /// </summary>
        public ReplyDto? ParentReply { get; set; }

        /// <summary>
        /// 所屬文章的基本資料
        /// </summary>
        public ArticleDto? Article { get; set; }

        /// <summary>
        /// 子回覆列表
        /// </summary>
        public List<ReplyDto> ChildReplies { get; set; } = new List<ReplyDto>();

        /// <summary>
        /// 獲取回覆狀態的描述文字
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
        /// </summary>
        public bool IsNormalStatus => Status == 0;

        /// <summary>
        /// 判斷是否為頂層回覆
        /// </summary>
        public bool IsTopLevel => ParentReplyId == null;

        /// <summary>
        /// 判斷是否為嵌套回覆
        /// </summary>
        public bool IsNested => ParentReplyId != null;

        /// <summary>
        /// 獲取回覆的簡短內容
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
        /// 獲取子回覆的數量
        /// </summary>
        public int ChildReplyCount => ChildReplies.Count;

        /// <summary>
        /// 判斷是否有子回覆
        /// </summary>
        public bool HasChildReplies => ChildReplyCount > 0;

        /// <summary>
        /// 獲取回覆的層級深度
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
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 判斷回覆是否受歡迎
        /// </summary>
        public bool IsPopular => PraiseCount >= 5;

        /// <summary>
        /// 獲取回覆的作者顯示名稱
        /// </summary>
        public string AuthorName => Author?.EffectiveDisplayName ?? "未知作者";

        /// <summary>
        /// 獲取回覆的作者頭像
        /// </summary>
        public string AuthorAvatar => !string.IsNullOrEmpty(Author?.AvatarPath) ? Author.AvatarPath : "/static/images/default_avatar.png";

        /// <summary>
        /// 獲取被回覆者的顯示名稱
        /// </summary>
        public string? ParentAuthorName => ParentReply?.AuthorName;
        
        /// <summary>
        /// 獲取回覆者的回覆時間
        /// </summary>
        public string CreateTimeFormatted => CreateTime.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 獲取回覆的完整顯示文字
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
        /// </summary>
        public bool CanBeDeleted => IsNormalStatus && !HasChildReplies;

        /// <summary>
        /// 獲取回覆的統計資訊
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
    /// 用於建立 Reply 的資料傳輸物件
    /// </summary>
    public class CreateReplyDto
    {
        /// <summary>
        /// 回覆所屬文章的唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "Reply_ArticleIdRequired")]
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 父回覆的唯一識別碼
        /// </summary>
        public Guid? ParentReplyId { get; set; }

        /// <summary>
        /// 回覆的內容文字
        /// </summary>
        [Required(ErrorMessage = "Reply_ContentRequired")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Reply_ContentLength1To1000")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 判斷是否為頂層回覆
        /// </summary>
        public bool IsTopLevel => ParentReplyId == null;

        /// <summary>
        /// 判斷是否為嵌套回覆
        /// </summary>
        public bool IsNested => ParentReplyId != null;

        /// <summary>
        /// 獲取回覆內容的字數
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 判斷內容是否為有效長度
        /// </summary>
        public bool IsValidLength => ContentLength >= 1 && ContentLength <= 1000;

        /// <summary>
        /// 獲取內容長度的狀態訊息
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
        /// </summary>
        public string ReplyTypeText => IsTopLevel ? "對文章的回覆" : "對回覆的回覆";

        /// <summary>
        /// 驗證回覆資料的完整性
        /// </summary>
        public bool IsValid => ArticleId != Guid.Empty && !string.IsNullOrWhiteSpace(Content) && IsValidLength;

        /// <summary>
        /// 獲取驗證錯誤訊息
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
        /// </summary>
        public void NormalizeContent()
        {
            if (!string.IsNullOrEmpty(Content))
            {
                Content = Content.Trim();

                Content = Content.Replace("\n", "\r\n");

                while (Content.Contains("\n\n\n"))
                {
                    Content = Content.Replace("\n\n\n", "\n\n");
                }
            }
        }

        /// <summary>
        /// 驗證父回覆 ID 的有效性
        /// </summary>
        public bool IsValidParentReplyId()
        {
            if (IsTopLevel) return true;

            return ParentReplyId.HasValue && ParentReplyId.Value != Guid.Empty;
        }

        /// <summary>
        /// 獲取回覆的建立提示訊息
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
        /// 判斷回覆是否包含提及
        /// </summary>
        public bool ContainsMention => !string.IsNullOrEmpty(Content) && Content.Contains("@");

        /// <summary>
        /// 獲取回覆中的提及列表
        /// </summary>
        public List<string> GetMentions()
        {
            var mentions = new List<string>();

            if (string.IsNullOrEmpty(Content)) return mentions;

            var words = Content.Split(new[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

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
        /// 轉換為建立 Reply 實體所需的基本資料
        /// </summary>
        public Dictionary<string, object> ToCreateData()
        {
            NormalizeContent();

            var data = new Dictionary<string, object>
            {
                ["ArticleId"] = ArticleId,
                ["Content"] = Content,
                ["Status"] = 0,
                ["CreateTime"] = DateTime.Now,
                ["PraiseCount"] = 0
            };

            if (ParentReplyId.HasValue && ParentReplyId.Value != Guid.Empty)
            {
                data["ParentReplyId"] = ParentReplyId.Value;
            }

            return data;
        }

        /// <summary>
        /// 驗證建立回覆的完整性
        /// </summary>
        public (bool IsValid, List<string> Errors) ValidateForCreation()
        {
            var errors = ValidationErrors;

            if (!IsValidParentReplyId())
            {
                errors.Add("嵌套回覆必須指定有效的父回覆");
            }

            return (errors.Count == 0, errors);
        }
    }
}
