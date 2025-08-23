using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    /// <summary>
    /// User 實體的資料傳輸物件
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// 使用者的唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者的權限等級
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [Required(ErrorMessage = "User_UserNameRequired")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "User_UserNameLength1To50")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [Required(ErrorMessage = "User_EmailRequired")]
        [EmailAddress(ErrorMessage = "User_EmailInvalid")]
        [StringLength(100, ErrorMessage = "User_EmailMaxLength100")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "User_CountryMaxLength100")]
        public string? Country { get; set; }

        /// <summary>
        /// 使用者的性別
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// 帳號的建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 使用者最後一次登入的時間
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 帳號狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 軟刪除標記，0表示未刪除，1表示已刪除
        /// </summary>
        public int IsDelete { get; set; }

        /// <summary>
        /// 關聯的個人資料
        /// </summary>
        public PersonDto? Person { get; set; }

        /// <summary>
        /// 獲取使用者狀態的描述文字
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "未信箱驗證",
            1 => "啟用",
            2 => "停用",
            3 => "封禁",
            _ => "未知"
        };

        /// <summary>
        /// 獲取使用者角色的描述文字
        /// </summary>
        public string RoleText => Role switch
        {
            0 => "一般使用者",
            1 => "管理員",
            2 => "超級管理員",
            _ => "未知"
        };

        /// <summary>
        /// 獲取性別的描述文字
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
        /// 判斷帳號是否為有效狀態（未刪除且狀態正常）
        /// </summary>
        public bool IsActive => Status == 1 && IsDelete == 0;

        /// <summary>
        /// 判斷是否為管理員
        /// </summary>
        public bool IsAdmin => Role >= 1;

        /// <summary>
        /// 判斷是否已被軟刪除
        /// </summary>
        public bool IsDeleted => IsDelete == 1;

        /// <summary>
        /// 獲取刪除狀態的描述文字
        /// </summary>
        public string DeleteStatusText => IsDelete switch
        {
            0 => "正常",
            1 => "已刪除",
            _ => "未知"
        };
    }

    /// <summary>
    /// 系統管理員資料
    /// </summary>
    public class AdminDto
    {
        /// <summary>
        /// 使用者的唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者的權限等級
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [Required(ErrorMessage = "User_UserNameRequired")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "User_UserNameLength1To50")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 關聯的個人暱稱
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [Required(ErrorMessage = "User_EmailRequired")]
        [EmailAddress(ErrorMessage = "User_EmailInvalid")]
        [StringLength(100, ErrorMessage = "User_EmailMaxLength100")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者最後一次登入的時間
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 帳號狀態
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 軟刪除標記，0表示未刪除，1表示已刪除
        /// </summary>
        public int IsDelete { get; set; }

        /// <summary>
        /// 關聯的個人頭像
        /// </summary>
        public string? AvatarPath { get; set; }
    }

    /// <summary>
    /// 使用者列表參數
    /// </summary>
    public class UserParamDto
    {
        public int pages { get; set; } = 1;
        public int pageSize { get; set; }   
    }

    /// <summary>
    /// 管理員篩選請求的資料傳輸物件
    /// </summary>
    public class AdminFilterDto
    {
        /// <summary>
        /// 頁碼（從1開始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 篩選條件
        /// </summary>
        public AdminFilters? Filters { get; set; }
    }

    /// <summary>
    /// 管理員篩選條件
    /// </summary>
    public class AdminFilters
    {
        /// <summary>
        /// 關鍵字搜尋（用戶名稱、信箱、顯示名稱）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 超級管理員篩選（true=僅超級管理員, false=僅一般管理員, null=全部）
        /// </summary>
        public bool? SuperAdmin { get; set; }

        /// <summary>
        /// 狀態篩選（true=已啟用, false=未啟用, null=全部）
        /// </summary>
        public bool? Status { get; set; }
    }

    /// <summary>
    /// 建立管理員帳號的資料傳輸物件
    /// </summary>
    public class CreateAdminDto
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 確認密碼
        /// </summary>
        public string PasswordConfirm { get; set; } = string.Empty;

        /// <summary>
        /// 管理員角色 (1=管理員, 2=超級管理員)，預設為管理員
        /// </summary>
        public int Role { get; set; } = 1;
    }

    /// <summary>
    /// 更新管理員帳號的資料傳輸物件
    /// </summary>
    public class UpdateAdminDto
    {
        /// <summary>
        /// 使用者名稱（可選）
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 電子郵件（可選）
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 新密碼（可選）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 確認新密碼（可選）
        /// </summary>
        public string? PasswordConfirm { get; set; }

        /// <summary>
        /// 管理員角色 (1=管理員, 2=超級管理員)
        /// </summary>
        public int? Role { get; set; }

        /// <summary>
        /// 帳號狀態 (0=未驗證, 1=啟用, 2=停用, 3=封禁)
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 軟刪除標記 (0=未刪除, 1=已刪除)
        /// </summary>
        public int? IsDelete { get; set; }
    }

    /// <summary>
    /// 分析使用者資料
    /// </summary>
    public class UserBasicDto
    {
        /// <summary>
        /// 使用者的唯一識別碼
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [Required(ErrorMessage = "User_UserNameRequired")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "User_UserNameLength1To50")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者最後一次登入的時間
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
    }

    /// <summary>
    /// 建立新使用者的資料傳輸物件
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [Required(ErrorMessage = "User_UserNameRequired")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "User_UserNameLength3To20")]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "User_UserNameAllowedChars")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [Required(ErrorMessage = "User_EmailRequired")]
        [EmailAddress(ErrorMessage = "User_EmailInvalid")]
        [StringLength(30, ErrorMessage = "User_EmailMaxLength30")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的密碼
        /// </summary>
        [Required(ErrorMessage = "User_PasswordRequired")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "User_PasswordLength8To20")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#@$!%*?&])[A-Za-z\d#@$!%*?&]{8,20}$",
            ErrorMessage = "User_PasswordComplexity")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 確認密碼
        /// </summary>
        [Required(ErrorMessage = "User_PasswordConfirmRequired")]
        [Compare("Password", ErrorMessage = "User_PasswordsMustMatch")]
        public string PasswordConfirm { get; set; } = string.Empty;

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "User_CountryMaxLength100")]
        public string? Country { get; set; }

        /// <summary>
        /// 使用者的性別
        /// </summary>
        [Range(0, 3, ErrorMessage = "User_GenderRange0To3")]
        public int? Gender { get; set; }

        /// <summary>
        /// 使用者的權限等級
        /// </summary>
        [Range(0, 2, ErrorMessage = "User_RoleRange0To2")]
        public int Role { get; set; } = 0;

        /// <summary>
        /// 使用者的顯示名稱（用於個人資料）
        /// </summary>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "User_DisplayNameLength1To50")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的隱私設定
        /// </summary>
        [Range(0, 1, ErrorMessage = "User_PrivacyRange0Or1")]
        public int IsPrivate { get; set; } = 0;

        /// <summary>
        /// 獲取性別的描述文字
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
        /// </summary>
        public bool IsPasswordStrong()
        {
            if (string.IsNullOrEmpty(Password)) return false;

            if (Password.Length < 6) return false;

            if (!Password.Any(char.IsLetter)) return false;

            if (!Password.Any(char.IsDigit)) return false;

            return true;
        }

        /// <summary>
        /// 驗證電子郵件格式
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
        /// </summary>
        public bool IsValid

        {
            get
            {
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                    return false;

                if (!IsPasswordStrong())
                    return false;

                if (Password != PasswordConfirm)
                    return false;

                if (!IsEmailValid())
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 獲取隱私設定的描述文字
        /// </summary>
        public string PrivacyText => IsPrivate switch
        {
            0 => "公開",
            1 => "私人",
            _ => "未知"
        };
    }

    /// <summary>
    /// 更新使用者資料的資料傳輸物件
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "User_UserNameLength1To50")]
        [RegularExpression(@"^[a-zA-Z0-9_\u4e00-\u9fa5]+$", ErrorMessage = "User_UserNameAllowedCharsWithChinese")]
        public string? UserName { get; set; }

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [EmailAddress(ErrorMessage = "User_EmailInvalid")]
        [StringLength(100, ErrorMessage = "User_EmailMaxLength100")]
        public string? Email { get; set; }

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "User_CountryMaxLength100")]
        public string? Country { get; set; }

        /// <summary>
        /// 使用者的性別
        /// </summary>
        [Range(0, 3, ErrorMessage = "User_GenderRange0To3")]
        public int? Gender { get; set; }

        /// <summary>
        /// 使用者的個人顯示名稱
        /// </summary>
        [StringLength(50, ErrorMessage = "User_DisplayNameMaxLength50")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的個人簡介
        /// </summary>
        [StringLength(300, ErrorMessage = "User_BioMaxLength300")]
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
        [Range(0, 1, ErrorMessage = "User_PrivacyRange0Or1")]
        public int? IsPrivate { get; set; }

        /// <summary>
        /// 使用者的區塊鏈錢包地址
        /// </summary>
        public string? WalletAddress { get; set; }

        /// <summary>
        /// 獲取性別的描述文字
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
        /// </summary>
        public bool IsEmailValid()
        {
            if (string.IsNullOrEmpty(Email)) return true;
            
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
        /// 驗證 URL 格式
        /// </summary>
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// 驗證輸入資料的完整性
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName) && (UserName.Length < 1 || UserName.Length > 50))
                    return false;

                if (!string.IsNullOrEmpty(Email) && !IsEmailValid())
                    return false;

                if (!string.IsNullOrEmpty(Country) && Country.Length > 100)
                    return false;

                if (Gender.HasValue && (Gender < 0 || Gender > 3))
                    return false;

                if (IsPrivate.HasValue && (IsPrivate < 0 || IsPrivate > 1))
                    return false;


                if (!string.IsNullOrEmpty(DisplayName) && DisplayName.Length > 50)
                    return false;

                if (!string.IsNullOrEmpty(Bio) && Bio.Length > 300)
                    return false;

                // 二進制資料檢查（可以添加檔案大小限制）
                if (AvatarPath != null && AvatarPath.Length > 5 * 1024 * 1024) // 5MB
                    return false;

                if (BannerPath != null && BannerPath.Length > 10 * 1024 * 1024) // 10MB
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 清理和格式化輸入資料
        /// </summary>
        public void TrimAndCleanData()
        {
            UserName = UserName?.Trim();
            Email = Email?.Trim().ToLower();
            Country = Country?.Trim();
            DisplayName = DisplayName?.Trim();
            Bio = Bio?.Trim();
            WalletAddress = WalletAddress?.Trim();
            
            if (string.IsNullOrEmpty(UserName)) UserName = null;
            if (string.IsNullOrEmpty(Email)) Email = null;
            if (string.IsNullOrEmpty(Country)) Country = null;
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = null;
            if (string.IsNullOrEmpty(Bio)) Bio = null;
            if (string.IsNullOrEmpty(WalletAddress)) WalletAddress = null;
        }

        /// <summary>
        /// 檢查是否有任何欄位被更新
        /// </summary>
        public bool HasAnyUpdate => !string.IsNullOrEmpty(UserName) ||
                                   !string.IsNullOrEmpty(Email) ||
                                   !string.IsNullOrEmpty(Country) ||
                                   Gender.HasValue ||
                                   !string.IsNullOrEmpty(DisplayName) ||
                                   !string.IsNullOrEmpty(Bio) ||
                                   (AvatarPath != null && AvatarPath.Length > 0) ||
                                   (BannerPath != null && BannerPath.Length > 0) ||
                                   IsPrivate.HasValue ||
                                   !string.IsNullOrEmpty(WalletAddress);

        /// <summary>
        /// 獲取所有驗證錯誤訊息
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
            
            if (!string.IsNullOrEmpty(DisplayName) && DisplayName.Length > 50)
            {
                errors.Add("顯示名稱長度不能超過 50 個字元");
            }
            
            if (!string.IsNullOrEmpty(Bio) && Bio.Length > 300)
            {
                errors.Add("個人簡介長度不能超過 300 個字元");
            }
            
            if (AvatarPath != null && AvatarPath.Length > 5 * 1024 * 1024)
            {
                errors.Add("頭像檔案大小不能超過 5MB");
            }
            
            if (BannerPath != null && BannerPath.Length > 10 * 1024 * 1024)
            {
                errors.Add("橫幅檔案大小不能超過 10MB");
            }
            
            if (IsPrivate.HasValue && (IsPrivate < 0 || IsPrivate > 1))
            {
                errors.Add("隱私設定必須是 0（公開）或 1（私人）");
            }
            
            return errors;
        }

        /// <summary>
        /// 獲取更新摘要
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
            
            if (!string.IsNullOrEmpty(DisplayName))
                updates.Add($"顯示名稱: {DisplayName}");
            
            if (!string.IsNullOrEmpty(Bio))
                updates.Add($"個人簡介: {Bio}");
            
            if (AvatarPath != null && AvatarPath.Length > 0)
                updates.Add($"頭像: 檔案大小 {AvatarPath.Length / 1024} KB");
            
            if (BannerPath != null && BannerPath.Length > 0)
                updates.Add($"橫幅: 檔案大小 {BannerPath.Length / 1024} KB");
            
            if (IsPrivate.HasValue)
                updates.Add($"隱私設定: {(IsPrivate == 0 ? "公開" : "私人")}");
            
            if (!string.IsNullOrEmpty(WalletAddress))
                updates.Add($"錢包地址: {WalletAddress}");
            
            return updates.Count > 0 ? string.Join(", ", updates) : "沒有要更新的資料";
        }
    }

    /// <summary>
    /// 用戶圖片資料傳輸物件
    /// </summary>
    public class UserImageDto
    {
        /// <summary>
        /// 附件檔案的 ID
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// 關聯文章的 ArticleId
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// 附件檔案的儲存路徑
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 附件的原始檔案名稱
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 附件的MIME類型
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// 文章建立時間（用於排序）
        /// </summary>
        public DateTime ArticleCreateTime { get; set; }
    }
}
