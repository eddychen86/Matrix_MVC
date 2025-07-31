using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Matrix.DTOs
{
    /// <summary>
    /// 用於透過 API 更新個人資料（包含檔案上傳）的資料傳輸物件
    /// </summary>
    public class ApiUpdateProfileDto
    {
        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [StringLength(50)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的個人簡介
        /// </summary>
        [StringLength(300)]
        public string? Bio { get; set; }

        /// <summary>
        /// 上傳的頭像檔案
        /// </summary>
        public IFormFile? AvatarFile { get; set; }

        /// <summary>
        /// 上傳的橫幅檔案
        /// </summary>
        public IFormFile? BannerFile { get; set; }
    }

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



        public string? Email { get; set; }

        public string? Website1 { get; set; }
        public string? Website2 { get; set; }
        public string? Website3 { get; set; }

        public List<String>? Content { get; set; }

        public List<ArticleDto> Articles { get; set; } = new List<ArticleDto>();

        /// <summary>
        /// 使用者密碼 - 在公開 API 中會被忽略
        /// </summary>
        [JsonIgnore]
        public string? Password { get; set; }

        public DateTime CreateTime { get; set; }

        

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
        /// 使用者頭像的檔案路徑
        /// </summary>
        [StringLength(2048)]
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 使用者個人頁面橫幅的檔案路徑
        /// </summary>
        [StringLength(2048)]
        public string? BannerPath { get; set; }

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
        public bool HasCustomAvatar => !string.IsNullOrEmpty(AvatarPath);

        /// <summary>
        /// 判斷是否有自訂橫幅
        /// </summary>
        public bool HasCustomBanner => !string.IsNullOrEmpty(BannerPath);

        /// <summary>
        /// 判斷是否有完整的個人資料
        /// </summary>
        public bool IsProfileComplete => !string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(Bio);


        /// <summary>
        /// 判斷是否有錢包地址
        /// </summary>
        public bool HasWallet => !string.IsNullOrEmpty(WalletAddress);


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
        /// 使用者頭像的檔案路徑
        /// </summary>
        [StringLength(2048)]
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 使用者個人頁面橫幅的檔案路徑
        /// </summary>
        [StringLength(2048)]
        public string? BannerPath { get; set; }

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
                   (!string.IsNullOrEmpty(AvatarPath)) ||
                   (!string.IsNullOrEmpty(BannerPath)) ||
                   IsPrivate.HasValue ||
                   !string.IsNullOrEmpty(WalletAddress);
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
            if (!string.IsNullOrEmpty(AvatarPath)) updates.Add("頭像");
            if (!string.IsNullOrEmpty(BannerPath)) updates.Add("橫幅");
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