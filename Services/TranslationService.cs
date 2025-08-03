namespace Matrix.Services
{
    /// <summary>
    /// 統一的翻譯服務 - 單一資料來源，避免重複維護
    /// </summary>
    public static class TranslationService
    {
        /// <summary>
        /// 靜態翻譯字典 - 唯一的翻譯資料來源
        /// 所有翻譯相關功能都應該使用這個字典
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, string>> AllTranslations = new()
        {
            ["zh-TW"] = new Dictionary<string, string>
            {
                #region Auth 模組

                // 標題
                ["LoginTitle"] = "登入",
                ["RegisterTitle"] = "註冊",
                ["ForgotPasswordTitle"] = "忘記密碼",

                // 副標題/訊息
                ["WelcomeMsg"] = "加入區塊鏈社群",
                ["ForgotPasswordMsg"] = "請輸入您的電子郵件地址以重置密碼。",
                ["NewHereMsg"] = "新來的？",
                ["hasAccountMsg"] = "已經有帳號？",

                // 標籤
                ["UserNameLabel"] = "用戶名",
                ["EmailLabel"] = "電子郵件",
                ["PasswordLabel"] = "密碼",
                ["ConfirmPasswordLabel"] = "確認密碼",
                ["RememberMeLabel"] = "記住我？",
                ["ForgotPasswordLabel"] = "忘記密碼？",
                ["SubmitBtn"] = "送出",

                // 輸入提示
                ["UserNamePlaceholder"] = "請輸入用戶名",
                ["EmailPlaceholder"] = "請輸入電子郵件",
                ["PasswordPlaceholder"] = "請輸入密碼",
                ["ConfirmPasswordPlaceholder"] = "請確認密碼",

                // 錯誤訊息
                ["UserNameInvalid"] = "用戶名未填寫",
                ["PasswordInvalid"] = "密碼未填寫",
                ["BothInvalid"] = "用戶名與密碼未填寫",
                ["UserNameFormatError"] = "用戶名需介於 3 到 20 個字，只能包含英文字母、數字和底線。",
                ["PasswordFormatError"] = "密碼需至少要有一個大寫字母、一個小寫字母、一個數字、一個特殊符號。",
                ["EmailFormatError"] = "電子郵件不得超過 30 個字。",
                ["EmailRequired"] = "電子郵件未填寫。",
                ["EmailInvalid"] = "請輸入有效的電子郵件地址。",
                ["PasswordLengthError"] = "密碼不得超過 20 個字。",
                ["UserNameLengthError"] = "用戶名不得超過 20 個字。",
                ["PasswordConfirmRequired"] = "確認密碼未填寫。",
                ["PasswordCompareError"] = "密碼不相符。",
                ["UsernameExists"] = "用戶名已被使用。",
                ["EmailExists"] = "電子郵件已被使用。",
                ["AccountLoginError"] = "用戶名或密碼不正確。",
                ["AccountNotVerified"] = "您的帳號尚未驗證，請先檢查您的電子郵件並完成帳號驗證。",
                ["AccountDisabled"] = "您的帳號已被停用，請聯繫管理員尋求協助。",

                #endregion

                #region Common 模組

                // 通用按鈕和操作
                ["OK"] = "確定",
                ["Cancel"] = "取消",
                ["Yes"] = "是",
                ["No"] = "否",
                ["Save"] = "儲存",
                ["Edit"] = "編輯",
                ["Delete"] = "刪除",
                ["Create"] = "建立",
                ["Submit"] = "提交",
                ["Loading"] = "載入中...",
                ["Error"] = "錯誤",
                ["Success"] = "成功",
                ["WelcomeMessage"] = "歡迎！",

                // 社交功能
                ["Comment"] = "留言",
                ["Praise"] = "讚",
                ["Follow"] = "追蹤",
                ["Collect"] = "收藏",
                ["Share"] = "分享",
                ["GuestBrowseMsg"] = "登入後可操作",
                ["GuestAccount"] = "訪客",

                #endregion

                #region Menu 模組

                ["Matrix"] = "Matrix",
                ["Login"] = "登入",
                ["Search"] = "搜尋",
                ["Notify"] = "通知",
                ["Follows"] = "追蹤",
                ["Collects"] = "收藏",
                ["Language"] = "繁體中文",
                ["HideBar"] = "隱藏側欄",
                ["LogOut"] = "登出",

                ["Dashboard"] = "後台",
                ["Overview"] = "總覽",
                ["Users"] = "用戶管理",
                ["Posts"] = "貼文管理",
                ["Reports"] = "檢舉管理",
                ["Logs"] = "網站日誌",

                #endregion

                #region Home 模組

                ["Home"] = "首頁",
                ["Welcome"] = "歡迎",
                ["HomePage"] = "首頁",
                ["Collected"] = "已收集",
                ["CollectButton"] = "收藏",
                ["Following"] = "已追蹤",
                ["FollowButton"] = "追蹤",
                ["SearchResults"] = "搜尋結果",
                ["SearchButton"] = "搜尋",
                ["NoArticlesMsg"] = "暫無文章內容",
                ["AllContentViewedTitle"] = "您已瀏覽完所有可用內容",
                ["LoginForMoreContentMsg"] = "登入後即可瀏覽更多精彩文章與留言討論",
                ["LoginNowBtn"] = "立即登入",

                #endregion

                #region Profile 模組
                ["ProfileName"] = "個人資料名稱",
                ["ProfileDescription"] = "個人資料描述",
                ["EditProfile"] = "編輯個人資料",
                ["SaveProfile"] = "儲存個人資料",

                #endregion

                #region Admin 模組

                ["DashboardTitle"] = "管理儀表板",
                ["TotalUsers"] = "使用者總數",
                ["TotalRecords"] = "記錄總數",
                ["TotalReports"] = "報告總數",
                ["UsersTitle"] = "使用者管理",
                ["UserID"] = "使用者編號",
                ["UserName"] = "使用者名稱",
                ["UserEmail"] = "使用者電子郵件",
                ["UserStatus"] = "使用者狀態",
                ["RecordsTitle"] = "記錄管理",
                ["RecordID"] = "記錄編號",
                ["RecordName"] = "記錄名稱",
                ["RecordDate"] = "記錄日期",
                ["RecordStatus"] = "記錄狀態",
                ["ReportsTitle"] = "報告管理",
                ["ReportID"] = "報告編號",
                ["ReportName"] = "報告名稱",
                ["ReportDate"] = "報告日期",
                ["ReportType"] = "報告類型",
                
                // === 確認模組 ===
                ["InvalidConfirmLink"] = "無效的確認連結",
                ["UserNotExistOrExpired"] = "用戶不存在或確認連結已失效",
                ["AccountAlreadyConfirmed"] = "您的帳號已經確認過了，可以直接登入",
                ["EmailConfirmSuccess"] = "郵件確認成功！您現在可以正常使用您的帳號了",
                ["ConfirmProcessError"] = "確認過程中發生錯誤，請稍後再試",
                ["UserNotExistPleaseRegister"] = "用戶不存在，請先註冊",
                ["WelcomeRegisterConfirmEmail"] = "歡迎註冊！請確認您的電子郵件地址",
                ["ConfirmEmailSent"] = "確認信已發送，請檢查您的電子郵件。",
                ["SendConfirmEmailError"] = "發送確認信時發生錯誤，請稍後再試。",
                
                // === 確認信內容 ===
                ["EmailWelcomeTitle"] = "歡迎加入 Matrix",
                ["EmailWelcomeSubtitle"] = "Welcome to the Matrix.",
                ["EmailGreeting"] = "嗨 {0}，",
                ["EmailMainContent"] = "感謝您註冊 Matrix 平台！這是一個為 Web3 先鋒和深度技術愛好者打造的純淨討論空間。為了確保您的帳戶安全，請點擊下方按鈕來驗證您的電子郵件地址。",
                ["EmailConfirmButton"] = "確認電子郵件",
                ["EmailAlternativeText"] = "如果按鈕無法點擊，請複製以下連結到瀏覽器：",
                ["EmailFooterText"] = "此連結將在 24 小時後失效。",
                ["EmailBrandMotto"] = "The world is a fog, filled with out-of-focus noise. We choose to become an eternal lighthouse.",
                
                // === 確認頁面 ===
                ["ConfirmPageTitle"] = "郵件確認",
                ["PleaseWait"] = "請稍候...",
                ["ProcessingConfirmRequest"] = "正在處理確認請求",
                ["VerifyingEmailLink"] = "驗證您的郵件確認連結中...",
                ["ConfirmSuccessTitle"] = "確認成功！",
                ["ConfirmFailedTitle"] = "確認失敗",
                ["EmailVerificationComplete"] = "郵件驗證完成",
                ["VerificationProblem"] = "驗證過程中發生問題",
                ["ProcessingConfirmError"] = "處理您的確認請求時發生了問題。",
                ["VerificationCompleteLabel"] = "驗證完成",
                ["CanUseFullFeatures"] = "您現在可以使用完整的 Matrix 平台功能了。",
                ["VerificationFailedLabel"] = "驗證失敗",
                ["CheckLinkOrContact"] = "請檢查您的確認連結是否有效，或聯繫客服人員協助。",
                ["GoToLogin"] = "前往登入",
                ["ReRegister"] = "重新註冊",
                ["BackToHome"] = "返回首頁",
                ["ProcessingResultError"] = "處理確認結果時發生錯誤。",
                ["CannotGetResult"] = "無法獲取確認結果，請重新嘗試。",
                ["UseConfirmLink"] = "請透過郵件中的確認連結來訪問此頁面。",
                
                // 其他常用
                ["Title"] = "Matrix",
                ["Email"] = "電子郵件",
                ["Password"] = "密碼",
                ["ConfirmPassword"] = "確認密碼",
                ["Register"] = "註冊"
                
                #endregion
            },
            
            ["en-US"] = new Dictionary<string, string>
            {
                #region Auth 模組
                // 標題
                ["LoginTitle"] = "Login",
                ["RegisterTitle"] = "Register",
                ["ForgotPasswordTitle"] = "Forgot Password",

                // 副標題/訊息
                ["WelcomeMsg"] = "Join the blockchain community",
                ["ForgotPasswordMsg"] = "Please enter your email address to reset your password.",
                ["NewHereMsg"] = "New here? Let ",
                ["hasAccountMsg"] = "Already have an account?",

                // 標籤
                ["UserNameLabel"] = "User Name",
                ["EmailLabel"] = "Email Address",
                ["PasswordLabel"] = "Password",
                ["ConfirmPasswordLabel"] = "Confirm Password",
                ["RememberMeLabel"] = "Remember me",
                ["ForgotPasswordLabel"] = "Forgot password?",
                ["SubmitBtn"] = "Submit",

                // 輸入提示
                ["UserNamePlaceholder"] = "Enter User Name",
                ["EmailPlaceholder"] = "Enter Email",
                ["PasswordPlaceholder"] = "Enter Password",
                ["ConfirmPasswordPlaceholder"] = "Confirm Password",

                // 錯誤訊息
                ["UserNameInvalid"] = "User name is required.",
                ["PasswordInvalid"] = "Password is required.",
                ["UserNameFormatError"] = "User name must be 3-20 characters, containing only letters, numbers, and underscores.",
                ["PasswordFormatError"] = "Password must contain at least one uppercase letter, lowercase letter, digit, and special character.",
                ["EmailFormatError"] = "Email must be less than 30 characters.",
                ["EmailRequired"] = "Email is required.",
                ["EmailInvalid"] = "Please enter a valid email address.",
                ["PasswordLengthError"] = "Password must not exceed 20 characters.",
                ["UserNameLengthError"] = "User name must not exceed 20 characters.",
                ["PasswordConfirmRequired"] = "Password confirmation is required.",
                ["PasswordCompareError"] = "Passwords do not match.",
                ["UsernameExists"] = "Username already exists.",
                ["EmailExists"] = "Email already exists.",
                ["AccountLoginError"] = "User Name or Password is incorrect.",
                ["AccountNotVerified"] = "Your account is not verified, please check your email and verify your account.",
                ["AccountDisabled"] = "Your account has been disabled, please contact the administrator for assistance.",

                #endregion

                #region Common 模組

                // 通用按鈕和操作
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Save"] = "Save",
                ["Edit"] = "Edit",
                ["Delete"] = "Delete",
                ["Create"] = "Create",
                ["Submit"] = "Submit",
                ["Loading"] = "Loading...",
                ["Error"] = "Error",
                ["Success"] = "Success",
                ["WelcomeMessage"] = "Welcome!",

                // 社交功能
                ["Comment"] = "Comment",
                ["Praise"] = "Praise",
                ["Follow"] = "Follow",
                ["Collect"] = "Collect",
                ["Share"] = "Share",
                ["GuestBrowseMsg"] = "Need to login",
                ["GuestAccount"] = "Guest",

                #endregion

                #region Menu 模組

                ["Matrix"] = "Matrix",
                ["Login"] = "Login",
                ["Search"] = "Search",
                ["Notify"] = "Notify",
                ["Follows"] = "Follows",
                ["Collects"] = "Collects",
                ["Language"] = "English",
                ["HideBar"] = "Hide bar",
                ["LogOut"] = "Log out",

                ["Dashboard"] = "Dashboard",
                ["Overview"] = "Overview",
                ["Users"] = "Users",
                ["Posts"] = "Posts",
                ["Reports"] = "Reports",
                ["Logs"] = "Logs",

                #endregion

                #region Home 模組

                ["Home"] = "Home",
                ["Welcome"] = "Welcome",
                ["HomePage"] = "Home Page",
                ["Collected"] = "Collected",
                ["CollectButton"] = "Collect",
                ["Following"] = "Following",
                ["FollowButton"] = "Follow",
                ["SearchResults"] = "Search Results",
                ["SearchButton"] = "Search",
                ["NoArticlesMsg"] = "No articles available",
                ["AllContentViewedTitle"] = "You have viewed all available content",
                ["LoginForMoreContentMsg"] = "Login to browse more exciting articles and join discussions",
                ["LoginNowBtn"] = "Login Now",

                #endregion

                #region Profile 模組

                ["ProfileName"] = "Profile Name",
                ["ProfileDescription"] = "Profile Description",
                ["EditProfile"] = "Edit Profile",
                ["SaveProfile"] = "Save Profile",

                #endregion

                #region Admin 模組

                ["DashboardTitle"] = "Admin Dashboard",
                ["TotalUsers"] = "Total Users",
                ["TotalRecords"] = "Total Records",
                ["TotalReports"] = "Total Reports",
                ["UsersTitle"] = "Users Management",
                ["UserID"] = "User ID",
                ["UserName"] = "User Name",
                ["UserEmail"] = "User Email",
                ["UserStatus"] = "User Status",
                ["RecordsTitle"] = "Records Management",
                ["RecordID"] = "Record ID",
                ["RecordName"] = "Record Name",
                ["RecordDate"] = "Record Date",
                ["RecordStatus"] = "Record Status",
                ["ReportsTitle"] = "Reports Management",
                ["ReportID"] = "Report ID",
                ["ReportName"] = "Report Name",
                ["ReportDate"] = "Report Date",
                ["ReportType"] = "Report Type",
                
                // === 確認模組 ===
                ["InvalidConfirmLink"] = "Invalid confirmation link",
                ["UserNotExistOrExpired"] = "User does not exist or confirmation link has expired",
                ["AccountAlreadyConfirmed"] = "Your account has already been confirmed, you can log in directly",
                ["EmailConfirmSuccess"] = "Email confirmation successful! You can now use your account normally",
                ["ConfirmProcessError"] = "An error occurred during the confirmation process, please try again later",
                ["UserNotExistPleaseRegister"] = "User does not exist, please register first",
                ["WelcomeRegisterConfirmEmail"] = "Welcome to register! Please confirm your email address",
                ["ConfirmEmailSent"] = "Confirmation email has been sent, please check your email.",
                ["SendConfirmEmailError"] = "An error occurred while sending confirmation email, please try again later.",
                
                // === 確認信內容 ===
                ["EmailWelcomeTitle"] = "Welcome to Matrix",
                ["EmailWelcomeSubtitle"] = "Welcome to the Matrix.",
                ["EmailGreeting"] = "Hi {0},",
                ["EmailMainContent"] = "Thank you for registering on the Matrix platform! This is a pure discussion space designed for Web3 pioneers and deep-tech enthusiasts. To ensure your account security, please click the button below to verify your email address.",
                ["EmailConfirmButton"] = "Confirm Email",
                ["EmailAlternativeText"] = "If the button doesn't work, please copy the following link to your browser:",
                ["EmailFooterText"] = "This link will expire in 24 hours.",
                ["EmailBrandMotto"] = "The world is a fog, filled with out-of-focus noise. We choose to become an eternal lighthouse.",
                
                // === 確認頁面 ===
                ["ConfirmPageTitle"] = "Email Confirmation",
                ["PleaseWait"] = "Please wait...",
                ["ProcessingConfirmRequest"] = "Processing confirmation request",
                ["VerifyingEmailLink"] = "Verifying your email confirmation link...",
                ["ConfirmSuccessTitle"] = "Confirmation Successful!",
                ["ConfirmFailedTitle"] = "Confirmation Failed",
                ["EmailVerificationComplete"] = "Email verification complete",
                ["VerificationProblem"] = "Problem occurred during verification",
                ["ProcessingConfirmError"] = "An error occurred while processing your confirmation request.",
                ["VerificationCompleteLabel"] = "Verification Complete",
                ["CanUseFullFeatures"] = "You can now use all Matrix platform features.",
                ["VerificationFailedLabel"] = "Verification Failed",
                ["CheckLinkOrContact"] = "Please check if your confirmation link is valid, or contact customer service for assistance.",
                ["GoToLogin"] = "Go to Login",
                ["ReRegister"] = "Re-register",
                ["BackToHome"] = "Back to Home",
                ["ProcessingResultError"] = "An error occurred while processing the confirmation result.",
                ["CannotGetResult"] = "Unable to get confirmation result, please try again.",
                ["UseConfirmLink"] = "Please access this page through the confirmation link in your email.",
                
                // 其他常用
                ["Title"] = "Matrix",
                ["Email"] = "Email",
                ["Password"] = "Password",
                ["ConfirmPassword"] = "Confirm Password",
                ["Register"] = "Register"
                
                #endregion
            }
        };

        /// <summary>
        /// 取得指定語言的翻譯字典
        /// </summary>
        /// <param name="culture">語言代碼（例如：zh-TW, en-US）</param>
        /// <returns>翻譯字典，如果語言不存在則返回中文字典</returns>
        public static Dictionary<string, string> GetTranslations(string culture)
        {
            if (AllTranslations.ContainsKey(culture))
            {
                return AllTranslations[culture];
            }
            
            // 回退到繁體中文
            return AllTranslations["zh-TW"];
        }

        /// <summary>
        /// 取得指定語言和鍵值的翻譯文字
        /// </summary>
        /// <param name="culture">語言代碼</param>
        /// <param name="key">翻譯鍵值</param>
        /// <returns>翻譯文字，如果找不到則返回鍵值本身</returns>
        public static string GetTranslation(string culture, string key)
        {
            var translations = GetTranslations(culture);
            
            if (translations.ContainsKey(key))
            {
                return translations[key];
            }
            
            // 如果當前語言找不到，嘗試從中文字典找
            if (culture != "zh-TW" && AllTranslations["zh-TW"].ContainsKey(key))
            {
                return AllTranslations["zh-TW"][key];
            }
            
            // 都找不到則返回鍵值本身
            return key;
        }

        /// <summary>
        /// 檢查是否支援指定的語言
        /// </summary>
        /// <param name="culture">語言代碼</param>
        /// <returns>是否支援</returns>
        public static bool IsSupportedCulture(string culture)
        {
            return AllTranslations.ContainsKey(culture);
        }

        /// <summary>
        /// 取得所有支援的語言代碼
        /// </summary>
        /// <returns>支援的語言代碼列表</returns>
        public static IEnumerable<string> GetSupportedCultures()
        {
            return AllTranslations.Keys;
        }
    }
}