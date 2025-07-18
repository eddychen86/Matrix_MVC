using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs;

/// <summary>
/// User 實體的資料傳輸物件 (Data Transfer Object)
/// 
/// 此檔案用於在不同層之間傳輸 User 實體的資料，包括：
/// - 用於 API 回應的資料格式
/// - 用於前端顯示的資料結構
/// - 用於服務層之間的資料傳遞
/// 
/// 注意事項：
/// - 僅能新增與 User 實體相關的屬性
/// - 不應包含敏感資訊（如密碼）
/// - 所有屬性都應該是前端可以安全顯示的
/// - 包含適當的 Data Annotations 進行驗證
/// - 此 DTO 主要用於讀取操作，不包含密碼等敏感資訊
/// </summary>
public class UserDto
{
    /// <summary>
    /// 使用者的唯一識別碼
    /// 用途：作為使用者的主要識別
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 使用者的權限等級
    /// 用途：前端根據此值決定使用者可存取的功能
    /// 值說明：0=一般使用者, 1=管理員, 2=超級管理員
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// 使用者的顯示名稱
    /// 用途：在前端界面中顯示的使用者名稱
    /// 驗證：必填，長度限制 1-50 個字元
    /// </summary>
    [Required(ErrorMessage = "使用者名稱為必填欄位")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "使用者名稱長度必須介於 1 到 50 個字元之間")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 使用者的電子郵件地址
    /// 用途：聯絡使用者和帳戶識別
    /// 驗證：必填，必須是有效的電子郵件格式
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者所在的國家
    /// 用途：地理位置資訊顯示
    /// 驗證：選填，最大長度 100 個字元
    /// </summary>
    [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
    public string? Country { get; set; }

    /// <summary>
    /// 使用者的性別
    /// 用途：個人資料顯示
    /// 值說明：0=未指定, 1=男性, 2=女性, 3=其他
    /// </summary>
    public int? Gender { get; set; }

    /// <summary>
    /// 帳號的建立時間
    /// 用途：顯示帳號註冊時間
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 使用者最後一次登入的時間
    /// 用途：顯示使用者活躍度
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 帳號狀態
    /// 用途：前端根據此值決定使用者帳號的可用性
    /// 值說明：0=正常, 1=停用, 2=封禁
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 關聯的個人資料
    /// 用途：包含使用者的詳細個人資訊
    /// </summary>
    public PersonDto? Person { get; set; }

    /// <summary>
    /// 獲取使用者狀態的描述文字
    /// 用途：在前端顯示人類可讀的狀態描述
    /// </summary>
    public string StatusText => Status switch
    {
        0 => "正常",
        1 => "停用",
        2 => "封禁",
        _ => "未知"
    };

    /// <summary>
    /// 獲取使用者角色的描述文字
    /// 用途：在前端顯示人類可讀的角色描述
    /// </summary>
    public string RoleText => Role switch
    {
        0 => "一般使用者",
        1 => "管理員",
        2 => "超級管理員",
        _ => "未知角色"
    };

    /// <summary>
    /// 獲取性別的描述文字
    /// 用途：在前端顯示人類可讀的性別描述
    /// </summary>
    public string GenderText => Gender switch
    {
        0 => "未指定",
        1 => "男性",
        2 => "女性",
        3 => "其他",
        null => "未指定",
        _ => "未知"
    };

    /// <summary>
    /// 判斷帳號是否為有效狀態
    /// 用途：前端快速判斷帳號可用性
    /// </summary>
    public bool IsActive => Status == 0;

    /// <summary>
    /// 判斷是否為管理員
    /// 用途：前端權限控制
    /// </summary>
    public bool IsAdmin => Role >= 1;
}

/// <summary>
/// 建立新使用者的資料傳輸物件 (Data Transfer Object)
/// 
/// 此檔案用於接收建立新使用者的資料，包括：
/// - 使用者註冊時的必要資訊
/// - 表單驗證規則
/// - 資料格式化和清理
/// 
/// 注意事項：
/// - 僅能新增與建立使用者相關的屬性
/// - 必須包含完整的資料驗證註解
/// - 不應包含系統自動生成的欄位（如 ID、建立時間等）
/// - 包含密碼確認欄位用於前端驗證
/// - 所有必填欄位都必須有適當的驗證規則
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// 使用者的顯示名稱
    /// 用途：使用者註冊時輸入的名稱
    /// 驗證：必填，長度限制 1-50 個字元，不能包含特殊字元
    /// </summary>
    [Required(ErrorMessage = "使用者名稱為必填欄位")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "使用者名稱長度必須介於 1 到 50 個字元之間")]
    [RegularExpression(@"^[a-zA-Z0-9_\u4e00-\u9fa5]+$", ErrorMessage = "使用者名稱只能包含字母、數字、底線和中文字元")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 使用者的電子郵件地址
    /// 用途：作為登入帳號和聯絡方式
    /// 驗證：必填，必須是有效的電子郵件格式，長度限制
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者的密碼
    /// 用途：帳號登入認證
    /// 驗證：必填，長度限制 6-100 個字元，必須包含字母和數字
    /// </summary>
    [Required(ErrorMessage = "密碼為必填欄位")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須介於 6 到 100 個字元之間")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$", 
        ErrorMessage = "密碼必須包含至少一個字母和一個數字")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 確認密碼
    /// 用途：確保使用者正確輸入密碼
    /// 驗證：必填，必須與密碼欄位相符
    /// </summary>
    [Required(ErrorMessage = "確認密碼為必填欄位")]
    [Compare("Password", ErrorMessage = "確認密碼必須與密碼相符")]
    public string PasswordConfirm { get; set; } = string.Empty;

    /// <summary>
    /// 使用者所在的國家
    /// 用途：地理位置資訊
    /// 驗證：選填，最大長度 100 個字元
    /// </summary>
    [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
    public string? Country { get; set; }

    /// <summary>
    /// 使用者的性別
    /// 用途：個人資料
    /// 驗證：選填，值必須在有效範圍內
    /// 值說明：0=未指定, 1=男性, 2=女性, 3=其他
    /// </summary>
    [Range(0, 3, ErrorMessage = "性別值必須在 0 到 3 之間")]
    public int? Gender { get; set; }

    /// <summary>
    /// 使用者的權限等級
    /// 用途：設定使用者的權限層級
    /// 驗證：選填，預設為 0（一般使用者）
    /// 值說明：0=一般使用者, 1=管理員, 2=超級管理員
    /// 注意：此欄位通常由系統管理員設定，一般註冊時不開放
    /// </summary>
    [Range(0, 2, ErrorMessage = "權限等級必須在 0 到 2 之間")]
    public int Role { get; set; } = 0;

    /// <summary>
    /// 使用者的顯示名稱（用於個人資料）
    /// 用途：在前端顯示的使用者名稱
    /// 驗證：選填，長度限制 1-50 個字元
    /// </summary>
    [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 使用者的隱私設定
    /// 用途：控制個人資料的可見性
    /// 驗證：選填，預設為 0（公開）
    /// 值說明：0=公開, 1=私人
    /// </summary>
    [Range(0, 1, ErrorMessage = "隱私設定必須是 0（公開）或 1（私人）")]
    public int IsPrivate { get; set; } = 0;

    /// <summary>
    /// 獲取性別的描述文字
    /// 用途：在前端顯示人類可讀的性別描述
    /// </summary>
    public string GenderText => Gender switch
    {
        0 => "未指定",
        1 => "男性",
        2 => "女性",
        3 => "其他",
        null => "未指定",
        _ => "未知"
    };

    /// <summary>
    /// 獲取使用者角色的描述文字
    /// 用途：在前端顯示人類可讀的角色描述
    /// </summary>
    public string RoleText => Role switch
    {
        0 => "一般使用者",
        1 => "管理員",
        2 => "超級管理員",
        _ => "未知角色"
    };

    /// <summary>
    /// 驗證密碼強度
    /// 用途：檢查密碼是否符合安全要求
    /// 回傳：true 表示密碼符合要求，false 表示不符合
    /// </summary>
    public bool IsPasswordStrong()
    {
        if (string.IsNullOrEmpty(Password)) return false;
        
        // 檢查長度
        if (Password.Length < 6) return false;
        
        // 檢查是否包含字母
        if (!Password.Any(char.IsLetter)) return false;
        
        // 檢查是否包含數字
        if (!Password.Any(char.IsDigit)) return false;
        
        return true;
    }

    /// <summary>
    /// 驗證電子郵件格式
    /// 用途：檢查電子郵件地址是否有效
    /// 回傳：true 表示格式有效，false 表示格式無效
    /// </summary>
    public bool IsEmailValid()
    {
        if (string.IsNullOrEmpty(Email)) return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(Email);
            return addr.Address == Email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 清理和格式化輸入資料
    /// 用途：移除不必要的空白字元和統一資料格式
    /// </summary>
    public void TrimAndCleanData()
    {
        UserName = UserName?.Trim() ?? string.Empty;
        Email = Email?.Trim().ToLower() ?? string.Empty;
        Password = Password?.Trim() ?? string.Empty;
        PasswordConfirm = PasswordConfirm?.Trim() ?? string.Empty;
        Country = Country?.Trim();
    }

    /// <summary>
    /// 驗證輸入資料的完整性
    /// 用途：檢查所有必填欄位是否符合要求
    /// </summary>
    public bool IsValid
    {
        get
        {
            // 檢查必填欄位
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                return false;

            // 檢查密碼強度
            if (!IsPasswordStrong())
                return false;

            // 檢查密碼確認
            if (Password != PasswordConfirm)
                return false;

            // 檢查電子郵件格式
            if (!IsEmailValid())
                return false;

            return true;
        }
    }

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
}

/// <summary>
/// 更新使用者資料的資料傳輸物件 (Data Transfer Object)
/// 
/// 此檔案用於接收更新使用者資料的資料，包括：
/// - 使用者更新個人資料時的資訊
/// - 表單驗證規則
/// - 資料格式化和清理
/// 
/// 注意事項：
/// - 僅能新增與更新使用者相關的屬性
/// - 所有欄位都是選填的，只更新有提供的欄位
/// - 必須包含適當的資料驗證註解
/// - 不應包含系統自動生成的欄位（如 ID、建立時間等）
/// - 不包含密碼更新（密碼更新應該有專門的方法）
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// 使用者的顯示名稱
    /// 用途：使用者更新時輸入的名稱
    /// 驗證：選填，如果提供則長度限制 1-50 個字元，不能包含特殊字元
    /// </summary>
    [StringLength(50, MinimumLength = 1, ErrorMessage = "使用者名稱長度必須介於 1 到 50 個字元之間")]
    [RegularExpression(@"^[a-zA-Z0-9_\u4e00-\u9fa5]+$", ErrorMessage = "使用者名稱只能包含字母、數字、底線和中文字元")]
    public string? UserName { get; set; }

    /// <summary>
    /// 使用者的電子郵件地址
    /// 用途：更新聯絡方式
    /// 驗證：選填，如果提供則必須是有效的電子郵件格式
    /// </summary>
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
    public string? Email { get; set; }

    /// <summary>
    /// 使用者所在的國家
    /// 用途：更新地理位置資訊
    /// 驗證：選填，最大長度 100 個字元
    /// </summary>
    [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
    public string? Country { get; set; }

    /// <summary>
    /// 使用者的性別
    /// 用途：更新個人資料
    /// 驗證：選填，值必須在有效範圍內
    /// 值說明：0=未指定, 1=男性, 2=女性, 3=其他
    /// </summary>
    [Range(0, 3, ErrorMessage = "性別值必須在 0 到 3 之間")]
    public int? Gender { get; set; }

    /// <summary>
    /// 獲取性別的描述文字
    /// 用途：在前端顯示人類可讀的性別描述
    /// </summary>
    public string GenderText => Gender switch
    {
        0 => "未指定",
        1 => "男性",
        2 => "女性",
        3 => "其他",
        null => "未指定",
        _ => "未知"
    };

    /// <summary>
    /// 驗證電子郵件格式
    /// 用途：檢查電子郵件地址是否有效
    /// 回傳：true 表示格式有效，false 表示格式無效
    /// </summary>
    public bool IsEmailValid()
    {
        if (string.IsNullOrEmpty(Email)) return true; // 選填欄位，空值也是有效的
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(Email);
            return addr.Address == Email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 驗證輸入資料的完整性
    /// 用途：檢查提供的資料是否符合基本要求
    /// </summary>
    public bool IsValid
    {
        get
        {
            // 如果使用者名稱有提供，檢查是否符合要求
            if (!string.IsNullOrEmpty(UserName) && (UserName.Length < 1 || UserName.Length > 50))
                return false;

            // 如果電子郵件有提供，檢查是否有效
            if (!string.IsNullOrEmpty(Email) && !IsEmailValid())
                return false;

            // 如果國家有提供，檢查長度
            if (!string.IsNullOrEmpty(Country) && Country.Length > 100)
                return false;

            // 如果性別有提供，檢查範圍
            if (Gender.HasValue && (Gender < 0 || Gender > 3))
                return false;

            return true;
        }
    }

    /// <summary>
    /// 清理和格式化輸入資料
    /// 用途：移除不必要的空白字元和統一資料格式
    /// </summary>
    public void TrimAndCleanData()
    {
        UserName = UserName?.Trim();
        Email = Email?.Trim().ToLower();
        Country = Country?.Trim();
        
        // 如果清理後變成空字串，則設為 null
        if (string.IsNullOrEmpty(UserName)) UserName = null;
        if (string.IsNullOrEmpty(Email)) Email = null;
        if (string.IsNullOrEmpty(Country)) Country = null;
    }

    /// <summary>
    /// 檢查是否有任何欄位被更新
    /// 用途：確認是否有提供任何要更新的資料
    /// </summary>
    public bool HasAnyUpdate => !string.IsNullOrEmpty(UserName) || 
                               !string.IsNullOrEmpty(Email) || 
                               !string.IsNullOrEmpty(Country) || 
                               Gender.HasValue;

    /// <summary>
    /// 獲取所有驗證錯誤訊息
    /// 用途：提供詳細的驗證錯誤資訊給前端
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (!string.IsNullOrEmpty(UserName) && (UserName.Length < 1 || UserName.Length > 50))
        {
            errors.Add("使用者名稱長度必須介於 1 到 50 個字元之間");
        }
        
        if (!string.IsNullOrEmpty(Email) && !IsEmailValid())
        {
            errors.Add("請輸入有效的電子郵件地址");
        }
        
        if (!string.IsNullOrEmpty(Country) && Country.Length > 100)
        {
            errors.Add("國家名稱長度不能超過 100 個字元");
        }
        
        if (Gender.HasValue && (Gender < 0 || Gender > 3))
        {
            errors.Add("性別值必須在 0 到 3 之間");
        }
        
        return errors;
    }

    /// <summary>
    /// 獲取更新摘要
    /// 用途：顯示將要更新的欄位摘要
    /// </summary>
    public string GetUpdateSummary()
    {
        var updates = new List<string>();
        
        if (!string.IsNullOrEmpty(UserName))
            updates.Add($"使用者名稱: {UserName}");
        
        if (!string.IsNullOrEmpty(Email))
            updates.Add($"電子郵件: {Email}");
        
        if (!string.IsNullOrEmpty(Country))
            updates.Add($"國家: {Country}");
        
        if (Gender.HasValue)
            updates.Add($"性別: {GenderText}");
        
        return updates.Count > 0 ? string.Join(", ", updates) : "沒有要更新的資料";
    }
}
