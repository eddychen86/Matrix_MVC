using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs;

/// <summary>
/// Person 實體的資料傳輸物件 (Data Transfer Object)
/// 
/// 此檔案用於在不同層之間傳輸 Person 實體的資料，包括：
/// - 用於 API 回應的資料格式
/// - 用於前端顯示的個人資料結構
/// - 用於服務層之間的資料傳遞
/// 
/// 注意事項：
/// - 僅能新增與 Person 實體相關的屬性
/// - 包含適當的 Data Annotations 進行驗證
/// - 此 DTO 主要用於讀取操作，顯示使用者的個人資料
/// - 包含計算屬性和輔助方法以增強前端使用體驗
/// </summary>
public class PersonDto
{
    /// <summary>
    /// 個人資料的唯一識別碼
    /// 用途：作為個人資料的主要識別
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    /// 關聯使用者的唯一識別碼
    /// 用途：連結到對應的使用者帳戶
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 使用者的顯示名稱
    /// 用途：在前端界面中顯示的使用者名稱
    /// 驗證：選填，長度限制 1-50 個字元
    /// </summary>
    [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 使用者的個人簡介
    /// 用途：展示使用者的自我介紹
    /// 驗證：選填，最大長度 300 個字元
    /// </summary>
    [StringLength(300, ErrorMessage = "個人簡介長度不能超過 300 個字元")]
    public string? Bio { get; set; }

    /// <summary>
    /// 使用者頭像的檔案路徑
    /// 用途：顯示使用者的個人頭像
    /// 驗證：選填，最大長度 2048 個字元
    /// </summary>
    [StringLength(2048, ErrorMessage = "頭像路徑長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "頭像路徑必須是有效的 URL")]
    public string? AvatarPath { get; set; }

    /// <summary>
    /// 使用者個人頁面橫幅的檔案路徑
    /// 用途：顯示使用者個人頁面的背景橫幅
    /// 驗證：選填，最大長度 2048 個字元
    /// </summary>
    [StringLength(2048, ErrorMessage = "橫幅路徑長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "橫幅路徑必須是有效的 URL")]
    public string? BannerPath { get; set; }

    /// <summary>
    /// 使用者的外部網站連結
    /// 用途：連結到使用者的個人網站或社交媒體
    /// 驗證：選填，最大長度 2048 個字元，必須是有效的 URL
    /// </summary>
    [StringLength(2048, ErrorMessage = "外部連結長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "外部連結必須是有效的 URL")]
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// 使用者的隱私設定
    /// 用途：控制個人資料的可見性
    /// 值說明：0=公開, 1=私人
    /// </summary>
    public int IsPrivate { get; set; }

    /// <summary>
    /// 使用者的區塊鏈錢包地址
    /// 用途：Web3 相關功能的錢包識別
    /// 驗證：選填，最大長度 100 個字元
    /// </summary>
    [StringLength(100, ErrorMessage = "錢包地址長度不能超過 100 個字元")]
    public string? WalletAddress { get; set; }

    /// <summary>
    /// 個人資料的最後修改時間
    /// 用途：顯示資料更新時間
    /// </summary>
    public DateTime? ModifyTime { get; set; }

    /// <summary>
    /// 關聯的使用者基本資料
    /// 用途：包含使用者的基本帳戶資訊
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// 獲取隱私設定的描述文字
    /// 用途：在前端顯示人類可讀的隱私設定描述
    /// </summary>
    public string PrivacyText => IsPrivate switch
    {
        0 => "公開",
        1 => "私人",
        _ => "未知"
    };

    /// <summary>
    /// 判斷是否為公開帳戶
    /// 用途：前端快速判斷帳戶可見性
    /// </summary>
    public bool IsPublic => IsPrivate == 0;

    /// <summary>
    /// 獲取顯示名稱或使用者名稱
    /// 用途：提供一個總是有值的顯示名稱
    /// 邏輯：優先使用 DisplayName，如果為空則使用 User.UserName
    /// </summary>
    public string EffectiveDisplayName => !string.IsNullOrEmpty(DisplayName) ? DisplayName : User?.UserName ?? "未知使用者";

    /// <summary>
    /// 獲取頭像 URL 或預設頭像
    /// 用途：確保總是有頭像可以顯示
    /// 邏輯：如果沒有設定頭像，則回傳預設頭像路徑
    /// </summary>
    public string EffectiveAvatarUrl => !string.IsNullOrEmpty(AvatarPath) ? AvatarPath : "/static/img/default-avatar.png";

    /// <summary>
    /// 獲取橫幅 URL 或預設橫幅
    /// 用途：確保總是有橫幅可以顯示
    /// 邏輯：如果沒有設定橫幅，則回傳預設橫幅路徑
    /// </summary>
    public string EffectiveBannerUrl => !string.IsNullOrEmpty(BannerPath) ? BannerPath : "/static/img/default-banner.jpg";

    /// <summary>
    /// 判斷是否有完整的個人資料
    /// 用途：檢查使用者是否已完成個人資料設定
    /// 邏輯：檢查關鍵欄位是否都有值
    /// </summary>
    public bool IsProfileComplete => !string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(Bio);

    /// <summary>
    /// 獲取個人資料完整度百分比
    /// 用途：顯示使用者完成個人資料的進度
    /// 計算：根據已填寫的欄位數量計算百分比
    /// </summary>
    public int ProfileCompletionPercentage
    {
        get
        {
            int totalFields = 5; // DisplayName, Bio, AvatarPath, BannerPath, ExternalUrl
            int completedFields = 0;

            if (!string.IsNullOrEmpty(DisplayName)) completedFields++;
            if (!string.IsNullOrEmpty(Bio)) completedFields++;
            if (!string.IsNullOrEmpty(AvatarPath)) completedFields++;
            if (!string.IsNullOrEmpty(BannerPath)) completedFields++;
            if (!string.IsNullOrEmpty(ExternalUrl)) completedFields++;

            return (completedFields * 100) / totalFields;
        }
    }

    /// <summary>
    /// 判斷是否有錢包地址
    /// 用途：檢查使用者是否設定了 Web3 錢包
    /// </summary>
    public bool HasWallet => !string.IsNullOrEmpty(WalletAddress);

    /// <summary>
    /// 獲取簡短的個人簡介
    /// 用途：在卡片或列表中顯示簡短的個人簡介
    /// 邏輯：如果個人簡介超過 100 個字元，則截取前 100 個字元並加上省略號
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
    /// 用途：在前端顯示人類可讀的時間格式
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
/// 更新個人資料的資料傳輸物件 (Data Transfer Object)
/// 
/// 此檔案用於接收更新個人資料的資料，包括：
/// - 使用者修改個人資料時的資訊
/// - 表單驗證規則
/// - 資料格式化和清理
/// 
/// 注意事項：
/// - 僅能新增與更新個人資料相關的屬性
/// - 必須包含完整的資料驗證註解
/// - 不應包含系統自動生成的欄位（如 ID、建立時間等）
/// - 所有欄位都是選填的，使用者可以選擇性更新
/// - 包含資料清理和驗證方法
/// </summary>
public class UpdatePersonDto
{
    /// <summary>
    /// 使用者的顯示名稱
    /// 用途：使用者想要修改的顯示名稱
    /// 驗證：選填，長度限制 1-50 個字元
    /// </summary>
    [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 使用者的個人簡介
    /// 用途：使用者想要修改的個人簡介
    /// 驗證：選填，最大長度 300 個字元
    /// </summary>
    [StringLength(300, ErrorMessage = "個人簡介長度不能超過 300 個字元")]
    public string? Bio { get; set; }

    /// <summary>
    /// 使用者頭像的檔案路徑
    /// 用途：使用者想要修改的頭像路徑
    /// 驗證：選填，最大長度 2048 個字元，必須是有效的 URL
    /// </summary>
    [StringLength(2048, ErrorMessage = "頭像路徑長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "頭像路徑必須是有效的 URL")]
    public string? AvatarPath { get; set; }

    /// <summary>
    /// 使用者個人頁面橫幅的檔案路徑
    /// 用途：使用者想要修改的橫幅路徑
    /// 驗證：選填，最大長度 2048 個字元，必須是有效的 URL
    /// </summary>
    [StringLength(2048, ErrorMessage = "橫幅路徑長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "橫幅路徑必須是有效的 URL")]
    public string? BannerPath { get; set; }

    /// <summary>
    /// 使用者的外部網站連結
    /// 用途：使用者想要修改的外部連結
    /// 驗證：選填，最大長度 2048 個字元，必須是有效的 URL
    /// </summary>
    [StringLength(2048, ErrorMessage = "外部連結長度不能超過 2048 個字元")]
    [Url(ErrorMessage = "外部連結必須是有效的 URL")]
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// 使用者的隱私設定
    /// 用途：使用者想要修改的隱私設定
    /// 驗證：選填，值必須在有效範圍內
    /// 值說明：0=公開, 1=私人
    /// </summary>
    [Range(0, 1, ErrorMessage = "隱私設定值必須在 0 到 1 之間")]
    public int? IsPrivate { get; set; }

    /// <summary>
    /// 使用者的區塊鏈錢包地址
    /// 用途：使用者想要修改的錢包地址
    /// 驗證：選填，最大長度 100 個字元
    /// </summary>
    [StringLength(100, ErrorMessage = "錢包地址長度不能超過 100 個字元")]
    public string? WalletAddress { get; set; }

    /// <summary>
    /// 獲取隱私設定的描述文字
    /// 用途：在前端顯示人類可讀的隱私設定描述
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
    /// 用途：檢查是否有任何資料需要更新
    /// 回傳：true 表示有欄位被設定，false 表示沒有任何變更
    /// </summary>
    public bool HasAnyChanges()
    {
        return !string.IsNullOrEmpty(DisplayName) ||
               !string.IsNullOrEmpty(Bio) ||
               !string.IsNullOrEmpty(AvatarPath) ||
               !string.IsNullOrEmpty(BannerPath) ||
               !string.IsNullOrEmpty(ExternalUrl) ||
               IsPrivate.HasValue ||
               !string.IsNullOrEmpty(WalletAddress);
    }

    /// <summary>
    /// 驗證所有 URL 欄位的有效性
    /// 用途：確保所有 URL 欄位都是有效的
    /// 回傳：包含無效 URL 欄位名稱的列表
    /// </summary>
    public List<string> ValidateUrls()
    {
        var invalidUrls = new List<string>();

        if (!string.IsNullOrEmpty(AvatarPath) && !IsValidUrl(AvatarPath))
            invalidUrls.Add("AvatarPath");

        if (!string.IsNullOrEmpty(BannerPath) && !IsValidUrl(BannerPath))
            invalidUrls.Add("BannerPath");

        if (!string.IsNullOrEmpty(ExternalUrl) && !IsValidUrl(ExternalUrl))
            invalidUrls.Add("ExternalUrl");

        return invalidUrls;
    }

    /// <summary>
    /// 檢查 URL 是否有效
    /// 用途：驗證 URL 格式的輔助方法
    /// </summary>
    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// 清理和格式化輸入資料
    /// 用途：移除不必要的空白字元和統一資料格式
    /// </summary>
    public void TrimAndCleanData()
    {
        DisplayName = DisplayName?.Trim();
        Bio = Bio?.Trim();
        AvatarPath = AvatarPath?.Trim();
        BannerPath = BannerPath?.Trim();
        ExternalUrl = ExternalUrl?.Trim();
        WalletAddress = WalletAddress?.Trim();

        // 將空字串轉換為 null
        if (string.IsNullOrEmpty(DisplayName)) DisplayName = null;
        if (string.IsNullOrEmpty(Bio)) Bio = null;
        if (string.IsNullOrEmpty(AvatarPath)) AvatarPath = null;
        if (string.IsNullOrEmpty(BannerPath)) BannerPath = null;
        if (string.IsNullOrEmpty(ExternalUrl)) ExternalUrl = null;
        if (string.IsNullOrEmpty(WalletAddress)) WalletAddress = null;
    }

    /// <summary>
    /// 獲取更新摘要
    /// 用途：生成更新內容的摘要，用於日誌記錄
    /// 回傳：描述更新內容的字串
    /// </summary>
    public string GetUpdateSummary()
    {
        var updates = new List<string>();

        if (!string.IsNullOrEmpty(DisplayName)) updates.Add("顯示名稱");
        if (!string.IsNullOrEmpty(Bio)) updates.Add("個人簡介");
        if (!string.IsNullOrEmpty(AvatarPath)) updates.Add("頭像");
        if (!string.IsNullOrEmpty(BannerPath)) updates.Add("橫幅");
        if (!string.IsNullOrEmpty(ExternalUrl)) updates.Add("外部連結");
        if (IsPrivate.HasValue) updates.Add("隱私設定");
        if (!string.IsNullOrEmpty(WalletAddress)) updates.Add("錢包地址");

        return updates.Count > 0 ? $"更新了：{string.Join(", ", updates)}" : "沒有變更";
    }

    /// <summary>
    /// 驗證錢包地址格式
    /// 用途：檢查區塊鏈錢包地址是否符合基本格式
    /// 回傳：true 表示格式可能有效，false 表示格式明顯無效
    /// 注意：這只是基本格式檢查，實際驗證需要根據具體的區塊鏈類型
    /// </summary>
    public bool IsWalletAddressValid()
    {
        if (string.IsNullOrEmpty(WalletAddress)) return true; // 選填欄位

        // 基本檢查：以太坊地址格式 (0x + 40 個十六進制字元)
        if (WalletAddress.StartsWith("0x") && WalletAddress.Length == 42)
        {
            return WalletAddress.Substring(2).All(c => 
                (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        // 其他格式可以在這裡添加檢查
        return false;
    }
}
