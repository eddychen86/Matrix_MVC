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
        [Required(ErrorMessage = "使用者名稱為必填欄位")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "使用者名稱長度必須介於 1 到 50 個字元之間")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [Required(ErrorMessage = "電子郵件為必填欄位")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
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
        /// 判斷帳號是否為有效狀態
        /// </summary>
        public bool IsActive => Status == 0;

        /// <summary>
        /// 判斷是否為管理員
        /// </summary>
        public bool IsAdmin => Role >= 1;
    }

    /// <summary>
    /// 建立新使用者的資料傳輸物件
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// 使用者的顯示名稱
        /// </summary>
        [Required(ErrorMessage = "使用者名稱為必填欄位")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "使用者名稱長度必須介於 3 到 20 個字元之間")]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "使用者名稱只能包含英文字母、數字和底線")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [Required(ErrorMessage = "電子郵件為必填欄位")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [StringLength(30, ErrorMessage = "電子郵件長度不能超過 30 個字元")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者的密碼
        /// </summary>
        [Required(ErrorMessage = "密碼為必填欄位")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼長度必須介於 8 到 20 個字元之間")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#@$!%*?&])[A-Za-z\d#@$!%*?&]{8,20}$",
            ErrorMessage = "密碼必須包含至少一個大寫字母、一個小寫字母、一個數字、一個特殊符號")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 確認密碼
        /// </summary>
        [Required(ErrorMessage = "確認密碼為必填欄位")]
        [Compare("Password", ErrorMessage = "確認密碼必須與密碼相符")]
        public string PasswordConfirm { get; set; } = string.Empty;

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
        public string? Country { get; set; }

        /// <summary>
        /// 使用者的性別
        /// </summary>
        [Range(0, 3, ErrorMessage = "性別值必須在 0 到 3 之間")]
        public int? Gender { get; set; }

        /// <summary>
        /// 使用者的權限等級
        /// </summary>
        [Range(0, 2, ErrorMessage = "權限等級必須在 0 到 2 之間")]
        public int Role { get; set; } = 0;

        /// <summary>
        /// 使用者的顯示名稱（用於個人資料）
        /// </summary>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "顯示名稱長度必須介於 1 到 50 個字元之間")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的隱私設定
        /// </summary>
        [Range(0, 1, ErrorMessage = "隱私設定必須是 0（公開）或 1（私人）")]
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
        [StringLength(50, MinimumLength = 1, ErrorMessage = "使用者名稱長度必須介於 1 到 50 個字元之間")]
        [RegularExpression(@"^[a-zA-Z0-9_\u4e00-\u9fa5]+$", ErrorMessage = "使用者名稱只能包含字母、數字、底線和中文字元")]
        public string? UserName { get; set; }

        /// <summary>
        /// 使用者的電子郵件地址
        /// </summary>
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
        public string? Email { get; set; }

        /// <summary>
        /// 使用者所在的國家
        /// </summary>
        [StringLength(100, ErrorMessage = "國家名稱長度不能超過 100 個字元")]
        public string? Country { get; set; }

        /// <summary>
        /// 使用者的性別
        /// </summary>
        [Range(0, 3, ErrorMessage = "性別值必須在 0 到 3 之間")]
        public int? Gender { get; set; }

        /// <summary>
        /// 使用者的個人顯示名稱
        /// </summary>
        [StringLength(50, ErrorMessage = "顯示名稱長度不能超過 50 個字元")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 使用者的個人簡介
        /// </summary>
        [StringLength(300, ErrorMessage = "個人簡介長度不能超過 300 個字元")]
        public string? Bio { get; set; }

        /// <summary>
        /// 使用者頭像的檔案路徑
        /// </summary>
        [StringLength(2048, ErrorMessage = "頭像路徑長度不能超過 2048 個字元")]
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 使用者個人頁面橫幅的檔案路徑
        /// </summary>
        [StringLength(2048, ErrorMessage = "橫幅路徑長度不能超過 2048 個字元")]
        public string? BannerPath { get; set; }

        /// <summary>
        /// 使用者的外部網站連結
        /// </summary>
        [StringLength(2048, ErrorMessage = "外部連結長度不能超過 2048 個字元")]
        [Url(ErrorMessage = "請輸入有效的 URL")]
        public string? ExternalUrl { get; set; }

        /// <summary>
        /// 使用者的隱私設定
        /// </summary>
        [Range(0, 1, ErrorMessage = "隱私設定必須是 0（公開）或 1（私人）")]
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

                if (!string.IsNullOrEmpty(ExternalUrl) && !IsValidUrl(ExternalUrl))
                    return false;

                if (!string.IsNullOrEmpty(DisplayName) && DisplayName.Length > 50)
                    return false;

                if (!string.IsNullOrEmpty(Bio) && Bio.Length > 300)
                    return false;

                if (!string.IsNullOrEmpty(AvatarPath) && AvatarPath.Length > 2048)
                    return false;

                if (!string.IsNullOrEmpty(BannerPath) && BannerPath.Length > 2048)
                    return false;

                if (!string.IsNullOrEmpty(ExternalUrl) && ExternalUrl.Length > 2048)
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
            AvatarPath = AvatarPath?.Trim();
            BannerPath = BannerPath?.Trim();
            ExternalUrl = ExternalUrl?.Trim();
            WalletAddress = WalletAddress?.Trim();
            
            if (string.IsNullOrEmpty(UserName)) UserName = null;
            if (string.IsNullOrEmpty(Email)) Email = null;
            if (string.IsNullOrEmpty(Country)) Country = null;
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = null;
            if (string.IsNullOrEmpty(Bio)) Bio = null;
            if (string.IsNullOrEmpty(AvatarPath)) AvatarPath = null;
            if (string.IsNullOrEmpty(BannerPath)) BannerPath = null;
            if (string.IsNullOrEmpty(ExternalUrl)) ExternalUrl = null;
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
                                   !string.IsNullOrEmpty(AvatarPath) ||
                                   !string.IsNullOrEmpty(BannerPath) ||
                                   !string.IsNullOrEmpty(ExternalUrl) ||
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
            
            if (!string.IsNullOrEmpty(AvatarPath) && AvatarPath.Length > 2048)
            {
                errors.Add("頭像路徑長度不能超過 2048 個字元");
            }
            
            if (!string.IsNullOrEmpty(BannerPath) && BannerPath.Length > 2048)
            {
                errors.Add("橫幅路徑長度不能超過 2048 個字元");
            }
            
            if (!string.IsNullOrEmpty(ExternalUrl) && (ExternalUrl.Length > 2048 || !IsValidUrl(ExternalUrl)))
            {
                errors.Add("外部連結必須是有效的 URL 且長度不能超過 2048 個字元");
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
            
            if (!string.IsNullOrEmpty(AvatarPath))
                updates.Add($"頭像路徑: {AvatarPath}");
            
            if (!string.IsNullOrEmpty(BannerPath))
                updates.Add($"橫幅路徑: {BannerPath}");
            
            if (!string.IsNullOrEmpty(ExternalUrl))
                updates.Add($"外部連結: {ExternalUrl}");
            
            if (IsPrivate.HasValue)
                updates.Add($"隱私設定: {(IsPrivate == 0 ? "公開" : "私人")}");
            
            if (!string.IsNullOrEmpty(WalletAddress))
                updates.Add($"錢包地址: {WalletAddress}");
            
            return updates.Count > 0 ? string.Join(", ", updates) : "沒有要更新的資料";
        }
    }
}