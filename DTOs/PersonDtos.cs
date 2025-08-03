using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// Person 實體的資料傳輸物件
    /// </summary>
    public class PersonDto
    {
        /// <summary>
        /// 個人資料的唯一識別碼
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// 關聯使用者的唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的個人簡介
        /// </summary>
        [StringLength(300, ErrorMessage = "個人簡介長度不能超過 300 個字元")]
        public string? Bio { get; set; }

        /// <summary>
        /// 使用者頭像的二進制資料
        /// </summary>
        public byte[]? AvatarPath { get; set; }

        /// <summary>
        /// 使用者個人頁面橫幅的二進制資料
        /// </summary>
        public byte[]? BannerPath { get; set; }

        /// <summary>
        /// 使用者的隱私設定
        /// </summary>
        public int IsPrivate { get; set; }

        /// <summary>
        /// 使用者的區塊鏈錢包地址
        /// </summary>
        [StringLength(100, ErrorMessage = "錢包地址長度不能超過 100 個字元")]
        public string? WalletAddress { get; set; }

        /// <summary>
        /// 個人資料的最後修改時間
        /// </summary>
        public DateTime? ModifyTime { get; set; }

        /// <summary>
        /// 關聯的使用者基本資料
        /// </summary>
        public UserDto? User { get; set; }

        /// <summary>
        /// 獲取隱私設定的描述文字
        /// </summary>
        public string PrivacyText => IsPrivate switch
        {
            0 => "公開",
            1 => "私人",
            _ => "未知"
        };

        /// <summary>
        /// 判斷是否為公開帳戶
        /// </summary>
        public bool IsPublic => IsPrivate == 0;

        /// <summary>
        /// 獲取顯示名稱或使用者名稱
        /// </summary>
        public string EffectiveDisplayName => !string.IsNullOrEmpty(DisplayName) ? DisplayName : User?.UserName ?? "未知使用者";

        /// <summary>
        /// 判斷是否有自訂頭像
        /// </summary>
        public bool HasCustomAvatar => AvatarPath != null && AvatarPath.Length > 0;

        /// <summary>
        /// 判斷是否有自訂橫幅
        /// </summary>
        public bool HasCustomBanner => BannerPath != null && BannerPath.Length > 0;

        /// <summary>
        /// 判斷是否有完整的個人資料
        /// </summary>
        public bool IsProfileComplete => !string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(Bio);

        /// <summary>
        /// 獲取個人資料完整度百分比
        /// </summary>
        public int ProfileCompletionPercentage
        {
            get
            {
                int totalFields = 4; // DisplayName, Bio, AvatarPath, BannerPath
                int completedFields = 0;

                if (!string.IsNullOrEmpty(DisplayName)) completedFields++;
                if (!string.IsNullOrEmpty(Bio)) completedFields++;
                if (AvatarPath != null && AvatarPath.Length > 0) completedFields++;
                if (BannerPath != null && BannerPath.Length > 0) completedFields++;

                return (completedFields * 100) / totalFields;
            }
        }

        /// <summary>
        /// 判斷是否有錢包地址
        /// </summary>
        public bool HasWallet => !string.IsNullOrEmpty(WalletAddress);

        /// <summary>
        /// 獲取簡短的個人簡介
        /// </summary>
        public string ShortBio
        {
            get
            {
                if (string.IsNullOrEmpty(Bio)) return "這個使用者還沒有個人簡介。";
                
                return Bio.Length > 100 ? Bio.Substring(0, 100) + "..." : Bio;
            }
        }

        /// <summary>
        /// 獲取最後修改時間的友善顯示格式
        /// </summary>
        public string LastModifiedText
        {
            get
            {
                if (ModifyTime == null) return "從未修改";
                
                var timeSpan = DateTime.Now - ModifyTime.Value;
                
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
    }

    /// <summary>
    /// 更新個人資料的資料傳輸物件
    /// </summary>
    public class UpdatePersonDto
    {
        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的個人簡介
        /// </summary>
        [StringLength(300, ErrorMessage = "個人簡介長度不能超過 300 個字元")]
        public string? Bio { get; set; }

        /// <summary>
        /// 使用者頭像的二進制資料
        /// </summary>
        public byte[]? AvatarPath { get; set; }

        /// <summary>
        /// 使用者個人頁面橫幅的二進制資料
        /// </summary>
        public byte[]? BannerPath { get; set; }

        /// <summary>
        /// 使用者的隱私設定
        /// </summary>
        [Range(0, 1, ErrorMessage = "隱私設定值必須在 0 到 1 之間")]
        public int? IsPrivate { get; set; }

        /// <summary>
        /// 使用者的區塊鏈錢包地址
        /// </summary>
        [StringLength(100, ErrorMessage = "錢包地址長度不能超過 100 個字元")]
        public string? WalletAddress { get; set; }

        /// <summary>
        /// 獲取隱私設定的描述文字
        /// </summary>
        public string PrivacyText => IsPrivate switch
        {
            0 => "公開",
            1 => "私人",
            null => "未變更",
            _ => "未知"
        };

        /// <summary>
        /// 判斷是否有任何欄位被設定
        /// </summary>
        public bool HasAnyChanges()
        {
            return !string.IsNullOrEmpty(DisplayName) ||
                   !string.IsNullOrEmpty(Bio) ||
                   (AvatarPath != null && AvatarPath.Length > 0) ||
                   (BannerPath != null && BannerPath.Length > 0) ||
                   IsPrivate.HasValue ||
                   !string.IsNullOrEmpty(WalletAddress);
        }

        /// <summary>
        /// 驗證二進制資料的有效性
        /// </summary>
        public List<string> ValidateBinaryData()
        {
            var invalidData = new List<string>();

            // 可以在這裡添加對二進制資料的驗證邏輯，例如檢查檔案大小、格式等
            if (AvatarPath != null && AvatarPath.Length > 5 * 1024 * 1024) // 5MB 限制
                invalidData.Add("AvatarPath: 檔案大小超過限制");

            if (BannerPath != null && BannerPath.Length > 10 * 1024 * 1024) // 10MB 限制
                invalidData.Add("BannerPath: 檔案大小超過限制");

            return invalidData;
        }


        /// <summary>
        /// 清理和格式化輸入資料
        /// </summary>
        public void TrimAndCleanData()
        {
            DisplayName = DisplayName?.Trim();
            Bio = Bio?.Trim();
            WalletAddress = WalletAddress?.Trim();

            if (string.IsNullOrEmpty(DisplayName)) DisplayName = null;
            if (string.IsNullOrEmpty(Bio)) Bio = null;
            if (string.IsNullOrEmpty(WalletAddress)) WalletAddress = null;
        }

        /// <summary>
        /// 獲取更新摘要
        /// </summary>
        public string GetUpdateSummary()
        {
            var updates = new List<string>();

            if (!string.IsNullOrEmpty(DisplayName)) updates.Add("顯示名稱");
            if (!string.IsNullOrEmpty(Bio)) updates.Add("個人簡介");
            if (AvatarPath != null && AvatarPath.Length > 0) updates.Add("頭像");
            if (BannerPath != null && BannerPath.Length > 0) updates.Add("橫幅");
            if (IsPrivate.HasValue) updates.Add("隱私設定");
            if (!string.IsNullOrEmpty(WalletAddress)) updates.Add("錢包地址");

            return updates.Count > 0 ? $"更新了：{string.Join(", ", updates)}" : "沒有變更";
        }

        /// <summary>
        /// 驗證錢包地址格式
        /// </summary>
        public bool IsWalletAddressValid()
        {
            if (string.IsNullOrEmpty(WalletAddress)) return true;

            if (WalletAddress.StartsWith("0x") && WalletAddress.Length == 42)
            {
                return WalletAddress.Substring(2).All(c =>
                    (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
            }

            return false;
        }
    }
}