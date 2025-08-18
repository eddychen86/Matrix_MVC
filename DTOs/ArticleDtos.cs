using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Article 實體的資料傳輸物件
    /// </summary>
    public class ArticleDto
    {
        /// <summary>
        /// 文章的唯一識別碼
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 文章作者的唯一識別碼
        /// </summary>
        public Guid AuthorId { get; set; }

        /// <summary>
        /// 文章的內容文字
        /// </summary>
        [Required(ErrorMessage = "Article_ContentRequired")]
        [StringLength(4000, ErrorMessage = "Article_ContentMaxLength4000")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 文章的公開狀態
        /// </summary>
        public int IsPublic { get; set; }

        /// <summary>
        /// 文章的狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 文章的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 文章獲得的讚數量
        /// </summary>
        public int PraiseCount { get; set; }

        /// <summary>
        /// 文章被收藏的次數
        /// </summary>
        public int CollectCount { get; set; }

        /// <summary>
        /// 文章作者的個人資料
        /// </summary>
        public PersonDto? Author { get; set; }

        /// <summary>
        /// 文章的回覆列表
        /// </summary>
        public List<ReplyDto> Replies { get; set; } = new List<ReplyDto>();

        /// <summary>
        /// 文章的附件列表
        /// </summary>
        public List<ArticleAttachmentDto> Attachments { get; set; } = new List<ArticleAttachmentDto>();

        /// <summary>
        /// 獲取文章狀態的描述文字
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
        /// 獲取文章可見性的描述文字
        /// 用途：在前端顯示人類可讀的可見性描述
        /// </summary>
        public string VisibilityText => IsPublic switch
        {
            0 => "公開",
            1 => "私人",
            _ => "未知"
        };

        /// <summary>
        /// 判斷文章是否為公開狀態
        /// 用途：前端快速判斷文章可見性
        /// </summary>
        public bool IsPublicArticle => IsPublic == 0;

        /// <summary>
        /// 判斷文章是否為正常狀態
        /// 用途：前端快速判斷文章是否可顯示
        /// </summary>
        public bool IsNormalStatus => Status == 0;

        /// <summary>
        /// 判斷文章是否可以顯示
        /// 用途：結合狀態和可見性判斷是否可顯示
        /// </summary>
        public bool IsVisible => IsNormalStatus && IsPublicArticle;

        /// <summary>
        /// 獲取文章的簡短內容
        /// 用途：在列表或卡片中顯示簡短的文章內容
        /// 邏輯：如果內容超過 150 個字元，則截取前 150 個字元並加上省略號
        /// </summary>
        public string ShortContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "此文章沒有內容。";

                return Content.Length > 150 ? Content.Substring(0, 150) + "..." : Content;
            }
        }


        /// <summary>
        /// 獲取文章的回覆數量
        /// 用途：顯示文章的回覆總數
        /// </summary>
        public int ReplyCount => Replies.Count;

        /// <summary>
        /// 判斷文章是否有回覆
        /// 用途：前端快速判斷是否需要顯示回覆區塊
        /// </summary>
        public bool HasReplies => ReplyCount > 0;

        /// <summary>
        /// 獲取文章的互動統計文字
        /// 用途：顯示文章的讚數、收藏數和回覆數的統計資訊
        /// </summary>
        public string InteractionSummary => $"{PraiseCount} 個讚，{CollectCount} 個收藏，{ReplyCount} 個回覆";

        /// <summary>
        /// 判斷文章是否受歡迎
        /// 用途：根據讚數和收藏數判斷文章是否受歡迎
        /// 邏輯：讚數超過 10 或收藏數超過 5 就算受歡迎
        /// </summary>
        public bool IsPopular => PraiseCount >= 10 || CollectCount >= 5;

        /// <summary>
        /// 獲取文章內容的字數
        /// 用途：顯示文章的字數統計
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 獲取預估的閱讀時間（以分鐘為單位）
        /// 用途：顯示文章的預估閱讀時間
        /// 邏輯：假設每分鐘閱讀 200 個字元
        /// </summary>
        public int EstimatedReadingTime
        {
            get
            {
                var wordsPerMinute = 200;
                var minutes = ContentLength / wordsPerMinute;
                return minutes < 1 ? 1 : minutes;
            }
        }

        /// <summary>
        /// 獲取文章的作者顯示名稱
        /// 用途：顯示文章作者的名稱
        /// </summary>
        public string AuthorName => Author?.EffectiveDisplayName ?? "未知作者";

        /// <summary>
        /// 獲取文章的作者頭像
        /// 用途：顯示文章作者的頭像
        /// </summary>
        public string AuthorAvatar => !string.IsNullOrEmpty(Author?.AvatarPath) ? Author.AvatarPath : "/static/images/default_avatar.png";
                
        /// <summary>
        /// 當前使用者是否已經點讚
        /// </summary>
        public bool IsPraised { get; set; }

        /// <summary>
        /// 當前使用者是否已經收藏
        /// </summary>
        public bool IsCollected { get; set; }
    }

    public class ArticleAttachmentDto
    {
        public Guid FileId { get; set; }
        public string FilePath { get; set; } = null!;
        public string? FileName { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 用於建立 Article 的資料傳輸物件 (Data Transfer Object)
    ///
    /// 此檔案用於在建立新文章時傳輸資料，包括：
    /// - 接收前端提交的新文章資料
    /// - 進行資料驗證
    /// - 傳遞給服務層處理
    ///
    /// 注意事項：
    /// - 僅能新增與建立 Article 相關的屬性
    /// - 包含完整的 Data Annotations 進行驗證
    /// - 此 DTO 專門用於建立操作，不包含系統生成的欄位
    /// - 包含適當的驗證規則確保資料完整性
    /// </summary>
    public class CreateArticleDto
    {
        /// <summary>
        /// 文章的內容文字
        /// 用途：使用者輸入的文章內容
        /// 驗證：必填，長度限制 1-4000 個字元
        /// </summary>
        [Required(ErrorMessage = "Article_ContentRequired")]
        [StringLength(4000, MinimumLength = 1, ErrorMessage = "Article_ContentLength1To4000")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 文章的公開狀態
        /// 用途：設定文章的可見性
        /// 值說明：0=公開, 1=私人
        /// 驗證：必須是 0 或 1
        /// </summary>
        [Range(0, 1, ErrorMessage = "Article_IsPublicRange0Or1")]
        public int IsPublic { get; set; } = 0;
        
        /// <summary>
        /// 要附加到文章的檔案類型
        /// </summary>
        public List<IFormFile>? Images { get; set; }
        public List<IFormFile>? Files { get; set; }
    

        /// <summary>
        /// 要附加到文章的檔案列表
        /// </summary>
        public List<IFormFile>? Attachments { get; set; }

        /// <summary>
        /// 選擇的標籤列表
        /// </summary>
        public List<string>? SelectedHashtags { get; set; }

        /// <summary>
        /// 獲取文章可見性的描述文字
        /// 用途：提供人類可讀的可見性描述
        /// </summary>
        public string VisibilityText => IsPublic switch
        {
            0 => "公開",
            1 => "私人",
            _ => "未知"
        };

        /// <summary>
        /// 判斷文章是否為公開狀態
        /// 用途：快速判斷文章可見性
        /// </summary>
        public bool IsPublicArticle => IsPublic == 0;

        /// <summary>
        /// 獲取文章內容的字數
        /// 用途：顯示文章的字數統計
        /// </summary>
        public int ContentLength => Content?.Length ?? 0;

        /// <summary>
        /// 獲取預估的閱讀時間（以分鐘為單位）
        /// 用途：顯示文章的預估閱讀時間
        /// 邏輯：假設每分鐘閱讀 200 個字元
        /// </summary>
        public int EstimatedReadingTime
        {
            get
            {
                var wordsPerMinute = 200;
                var minutes = ContentLength / wordsPerMinute;
                return minutes < 1 ? 1 : minutes;
            }
        }

        /// <summary>
        /// 判斷內容是否為有效長度
        /// 用途：前端即時驗證內容長度
        /// </summary>
        public bool IsValidLength => ContentLength >= 1 && ContentLength <= 4000;

        /// <summary>
        /// 獲取內容長度的狀態訊息
        /// 用途：提供內容長度的即時反饋
        /// </summary>
        public string ContentLengthStatus
        {
            get
            {
                if (ContentLength == 0) return "請輸入文章內容";
                if (ContentLength > 4000) return $"內容過長，已超過 {ContentLength - 4000} 個字元";
                if (ContentLength < 10) return "內容太短，建議至少 10 個字元";
                return $"內容長度正常（{ContentLength}/4000 字元）";
            }
        }

        /// <summary>
        /// 獲取內容的預覽文字
        /// 用途：在確認頁面顯示文章內容的預覽
        /// 邏輯：如果內容超過 100 個字元，則截取前 100 個字元並加上省略號
        /// </summary>
        public string PreviewContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "無內容";

                return Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;
            }
        }

        /// <summary>
        /// 驗證文章資料的完整性
        /// 用途：在提交前進行整體驗證
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Content) && IsValidLength;

        /// <summary>
        /// 獲取驗證錯誤訊息
        /// 用途：提供詳細的驗證錯誤資訊
        /// </summary>
        public List<string> ValidationErrors
        {
            get
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(Content))
                {
                    errors.Add("文章內容不能為空");
                }

                if (!IsValidLength)
                {
                    if (ContentLength > 4000)
                    {
                        errors.Add($"文章內容過長，超過 {ContentLength - 4000} 個字元");
                    }
                    else if (ContentLength < 1)
                    {
                        errors.Add("文章內容不能為空");
                    }
                }

                if (IsPublic < 0 || IsPublic > 1)
                {
                    errors.Add("文章狀態必須是 0（公開）或 1（私人）");
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
        /// 將 CreateArticleDto 轉換為建立 Article 實體所需的基本資料
        /// 用途：提供轉換方法供服務層使用
        /// </summary>
        public Dictionary<string, object> ToCreateData()
        {
            NormalizeContent();

            return new Dictionary<string, object>
            {
                ["Content"] = Content,
                ["IsPublic"] = IsPublic,
                ["Status"] = 0, // 新文章預設為正常狀態
                ["CreateTime"] = DateTime.Now,
                ["PraiseCount"] = 0,
                ["CollectCount"] = 0
            };
        }
    }
}
