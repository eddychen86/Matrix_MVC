namespace Matrix.Services
{
    /// <summary>
    /// 多國語系
    /// </summary>
    public static class TranslationService
    {
        public static readonly Dictionary<string, Dictionary<string, string>> AllTranslations = new()
        {
            ["zh-TW"] = new Dictionary<string, string>
            {
                #region footer 模組

                ["About"] = "關於我們",
                ["ContactUs"] = "聯繫我們",
                ["Disclaimer"] = "免責聲明",

                #endregion

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
                ["GuestBrowseMsg"] = "請先登入以進行互動",
                ["GuestAccount"] = "訪客",

                // 狀態
                ["All"] = "全部",
                ["enable"] = "啟用",
                ["disable"] = "停用",
                ["public"] = "公開",
                ["private"] = "不公開",

                ["bestResolution"] = "螢幕解析度建議 1920x1080",

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
                ["Config"] = "網站設置",

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

                ["ProfileBio"] = "自我介紹",
                ["ProfileContact"] = "聯絡方式",
                ["ProfileLinks"] = "其他連結",
                ["ProfileNFTs"] = "NFTs 戰略品",
                ["ProfileName"] = "個人資料名稱",
                ["ProfileDescription"] = "個人資料描述",
                ["EditProfile"] = "編輯個人資料",
                ["SaveProfile"] = "儲存個人資料",
                ["ProfileWellat"] = "錢包地址",
                ["ProfileState"] = "是否公開",
                ["ProfilePwd"] = "重設密碼",

                #endregion

                #region Friends 模組

                ["Friends"] = "好友",
                ["FriendsTitle"] = "好友列表",
                ["FriendsAccepted"] = "已接受",
                ["FriendsPending"] = "待處裡",
                ["FriendsDeclined"] = "已拒絕",
                ["FriendsBlocked"] = "封鎖",
                ["FriendsAll"] = "全部",
                ["FriendsNone"] = "尚無好友",

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
                ["Register"] = "註冊",

                #endregion

                // 驗證錯誤訊息（來自 DTOs）
                #region User
                ["User_UserNameRequired"] = "使用者名稱為必填欄位",
                ["User_UserNameLength1To50"] = "使用者名稱長度必須介於 1 到 50 個字元之間",
                ["User_EmailRequired"] = "電子郵件為必填欄位",
                ["User_EmailInvalid"] = "請輸入有效的電子郵件地址",
                ["User_EmailMaxLength100"] = "電子郵件長度不能超過 100 個字元",
                ["User_CountryMaxLength100"] = "國家名稱長度不能超過 100 個字元",
                ["User_UserNameLength3To20"] = "使用者名稱長度必須介於 3 到 20 個字元之間",
                ["User_UserNameAllowedChars"] = "使用者名稱只能包含英文字母、數字和底線",
                ["User_EmailMaxLength30"] = "電子郵件長度不能超過 30 個字元",
                ["User_PasswordRequired"] = "密碼為必填欄位",
                ["User_PasswordLength8To20"] = "密碼長度必須介於 8 到 20 個字元之間",
                ["User_PasswordComplexity"] = "密碼必須包含至少一個大寫字母、一個小寫字母、一個數字、一個特殊符號",
                ["User_PasswordConfirmRequired"] = "確認密碼為必填欄位",
                ["User_PasswordsMustMatch"] = "確認密碼必須與密碼相符",
                ["User_GenderRange0To3"] = "性別值必須在 0 到 3 之間",
                ["User_RoleRange0To2"] = "權限等級必須在 0 到 2 之間",
                ["User_DisplayNameMaxLength50"] = "顯示名稱長度不能超過 50 個字元",
                ["User_DisplayNameLength1To50"] = "顯示名稱長度必須介於 1 到 50 個字元之間",
                ["User_BioMaxLength300"] = "個人簡介長度不能超過 300 個字元",
                ["User_PrivacyRange0Or1"] = "隱私設定必須是 0（公開）或 1（私人）",
                ["User_UserNameAllowedCharsWithChinese"] = "使用者名稱只能包含字母、數字、底線和中文字元",

                // Avatar upload
                ["Avatar"] = "頭像",
                ["ClickToUploadAvatar"] = "點擊圖片上傳頭像",
                ["SupportedImageFormats"] = "支援 JPG, PNG, GIF (最大5MB)",
                #endregion

                #region Person
                ["Person_DisplayNameLength1To50"] = "顯示名稱長度必須介於 1 到 50 個字元之間",
                ["Person_BioMaxLength300"] = "個人簡介長度不能超過 300 個字元",
                ["Person_PrivacyRange0To1"] = "隱私設定值必須在 0 到 1 之間",
                ["Person_WalletAddressMaxLength100"] = "錢包地址長度不能超過 100 個字元",
                #endregion

                #region Notification
                ["Notification_TitleRequired"] = "通知標題為必填欄位",
                ["Notification_TitleMaxLength100"] = "通知標題長度不能超過 100 個字元",
                ["Notification_ContentMaxLength500"] = "通知內容長度不能超過 500 個字元",
                #endregion

                #region Article
                ["Article_ContentRequired"] = "文章內容為必填欄位",
                ["Article_ContentMaxLength4000"] = "文章內容長度不能超過 4000 個字元",
                ["Article_ContentLength1To4000"] = "文章內容長度必須介於 1 到 4000 個字元之間",
                ["Article_IsPublicRange0Or1"] = "文章狀態必須是 0（公開）或 1（私人）",
                #endregion

                #region Reply
                ["Reply_ArticleIdRequired"] = "文章 ID 為必填欄位",
                ["Reply_ContentRequired"] = "回覆內容為必填欄位",
                ["Reply_ContentMaxLength1000"] = "回覆內容長度不能超過 1000 個字元",
                ["Reply_ContentLength1To1000"] = "回覆內容長度必須介於 1 到 1000 個字元之間",
                #endregion

                #region NFT
                ["NFT_OwnerIdRequired"] = "擁有者 ID 為必填",
                ["NFT_FileNameRequired"] = "NFT 名稱為必填",
                ["NFT_FileNameMaxLength255"] = "NFT 名稱長度不能超過 255 個字元",
                ["NFT_FilePathRequired"] = "檔案路徑為必填",
                ["NFT_FilePathMaxLength2048"] = "檔案路徑長度不能超過 2048 個字元",
                ["NFT_CollectTimeRequired"] = "收藏時間為必填",
                ["NFT_CurrencyRequired"] = "幣別為必填",
                ["NFT_CurrencyMaxLength10"] = "幣別長度不能超過 10 個字元",
                ["NFT_PriceMin0"] = "價格必須大於等於 0",
                ["NFT_MinPriceMin0"] = "最低價格必須大於等於 0",
                ["NFT_MaxPriceMin0"] = "最高價格必須大於等於 0",
                ["NFT_PageMin1"] = "頁數必須大於 0",
                ["NFT_PageSizeRange1To100"] = "每頁筆數必須在 1-100 之間",

                #endregion

                #region Users Management 模組

                ["Users_Title"] = "使用者管理",
                ["Users_Status"] = "狀態",
                ["Users_Ban"] = "被封禁",
                ["Users_CreateTime"] = "建立時間",
                ["Users_Enable"] = "已啟用",
                ["Users_Disable"] = "未啟用",
                ["Users_Yes"] = "是",
                ["Users_No"] = "否",
                ["Users_Name"] = "名稱",
                ["Users_RoleText"] = "角色",
                ["Users_Email"] = "電子郵件",
                ["Users_LastLoginTime"] = "最後登入",
                ["Users_Edit"] = "編輯",
                ["Users_Delete"] = "刪除",
                ["Users_Update"] = "更新",
                ["Users_Cancel"] = "取消",
                ["Users_SearchUserPlaceholder"] = "請輸入使用者名稱",
                ["Users_SearchDatePlaceholder"] = "請輸入日期YYYY-MM-DD",
                ["Users_Status_Disabled"] = "未啟用",
                ["Users_Status_Enabled"] = "已啟用",
                ["Users_Status_Banned"] = "被封禁",
                ["Users_Role_Admin"] = "管理員",
                ["Users_Role_User"] = "一般使用者",

                #endregion

                #region Posts Management 模組

                ["Posts_Title"] = "貼文管理",
                ["Posts_Keyword"] = "關鍵字",
                ["Posts_SearchKeywordPlaceholder"] = "請輸入關鍵字",
                ["Posts_Status"] = "狀態",
                ["Posts_All"] = "全部",
                ["Posts_Enable"] = "啟用",
                ["Posts_Disable"] = "停用",
                ["Posts_CreateTime"] = "建立時間",
                ["Posts_Content"] = "內容",
                ["Posts_UserName"] = "使用者名稱",
                ["Posts_ModifyTime"] = "修改時間",
                ["Posts_Edit"] = "編輯",
                ["Posts_Delete"] = "刪除",
                ["Posts_Status_Normal"] = "正常",
                ["Posts_Status_Hidden"] = "隱藏",
                ["Posts_Status_Deleted"] = "已刪除",
                ["Posts_Save"] = "儲存",
                ["Posts_Cancel"] = "取消",
                ["Posts_ConfirmDelete"] = "確定要刪除這篇文章嗎?",
                ["Posts_DeleteError"] = "刪除失敗",
                ["Posts_UpdateError"] = "狀態更新失敗",
                ["Posts_LoadError"] = "讀取清單失敗",

                #endregion

                #region Reports Management 模組

                ["Reports_Title"] = "檢舉管理",
                ["Reports_Reason"] = "原因",
                ["Reports_Reporter"] = "檢舉者",
                ["Reports_Type"] = "類別",
                ["Reports_Target"] = "被檢舉目標",
                ["Reports_ModifyTime"] = "更新時間",
                ["Reports_Status"] = "狀態",
                ["Reports_Resolver"] = "處理者",

                #endregion

                #region Overview 模組

                ["Overview_Title"] = "管理儀表板",
                ["Overview_WelcomeMsg"] = "歡迎使用 Matrix 後台管理系統",
                ["Overview_TotalUsers"] = "總用戶數",
                ["Overview_TotalPosts"] = "文章總數",
                ["Overview_PendingReports"] = "待處理報告",
                ["Overview_TodayActive"] = "今日活躍",
                ["Overview_QuickActions"] = "快速操作",
                ["Overview_UserManagement"] = "用戶管理",
                ["Overview_PostManagement"] = "文章管理",
                ["Overview_ReportManagement"] = "檢舉管理",
                ["Overview_SystemStatus"] = "系統狀態",
                ["Overview_SystemUptime"] = "系統運行時間",
                ["Overview_DatabaseConnection"] = "資料庫連線",
                ["Overview_EmailService"] = "郵件服務",
                ["Overview_Storage"] = "儲存空間",
                ["Overview_Status_Normal"] = "正常",
                ["Overview_Status_InUse"] = "使用中",

                #endregion

                #region Config 模組

                ["Config_Title"] = "系統設定",
                ["Config_Add_Admin"] = "新增管理員",
                ["Config_Filter"] = "列表篩選",
                ["Config_ToolsState"] = "功能開關",
                ["Config_WebLog"] = "網站日誌",
                ["Config_AdminList"] = "管理員列表",
                ["Config_AdminList_Keyword"] = "關鍵字查詢",
                ["Config_AdminList_KeywordPlaceholder"] = "請輸入帳號或匿名",
                ["Config_AdminList_UserName"] = "帳號",
                ["Config_AdminList_DisplayName"] = "匿名",
                ["Config_AdminList_Email"] = "信箱",
                ["Config_AdminList_Admin"] = "一般管理員",
                ["Config_AdminList_SuperAdmin"] = "超級管理員",
                ["Config_AdminList_Status"] = "狀態",

                ["Config_LoginList"] = "管理員監控",
                ["Config_LoginList_Keyword"] = "關鍵字查詢",
                ["Config_LoginList_IP"] = "IP",
                ["Config_LoginList_Role"] = "角色",
                ["Config_LoginList_Role_Admin"] = "一般管理員",
                ["Config_LoginList_Role_SuperAdmin"] = "超級管理員",
                ["Config_LoginList_Account"] = "帳號",
                ["Config_LoginList_Browser"] = "瀏覽器資訊",
                ["Config_LoginList_PagePath"] = "頁面路徑",
                ["Config_LoginList_LoginTime"] = "登入時間",
                ["Config_LoginList_LogoutTime"] = "登出時間",
                ["Config_LoginList_StartTime"] = "事件時間",
                ["Config_LoginList_DurationTime"] = "停留時間(秒)",
                ["Config_LoginList_ActionType"] = "操作類型",
                ["Config_LoginList_ActionType_VIEW"] = "檢視",
                ["Config_LoginList_ActionType_CREATE"] = "新增",
                ["Config_LoginList_ActionType_UPDATE"] = "更新",
                ["Config_LoginList_ActionType_DELETE"] = "刪除",
                ["Config_LoginList_ActionType_ERROR"] = "錯誤",
                
                // 共用按鈕和操作
                ["Apply"] = "套用",
                ["Clear"] = "清除",

                #endregion
            },

            ["en-US"] = new Dictionary<string, string>
            {
                #region footer 模組

                ["About"] = "About",
                ["ContactUs"] = "Contact Us",
                ["Disclaimer"] = "Disclaimer",

                #endregion

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

                // 狀態
                ["All"] = "All",
                ["enable"] = "Enable",
                ["disable"] = "Disable",
                ["public"] = "Public",
                ["private"] = "Private",

                ["bestResolution"] = "Suggested resolution: 1920x1080",

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
                ["Config"] = "Settings",

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

                ["ProfileBio"] = "Bio",
                ["ProfileContact"] = "Contact",
                ["ProfileLinks"] = "Links",
                ["ProfileNFTs"] = "NFT Collects",
                ["ProfileName"] = "Profile Name",
                ["ProfileDescription"] = "Profile Description",
                ["EditProfile"] = "Edit Profile",
                ["SaveProfile"] = "Save Profile",
                ["ProfileWellat"] = "Wallet Address",
                ["ProfileState"] = "Profile State",
                ["ProfilePwd"] = "Reset Password",

                #endregion

                #region Friends 模組

                ["Friends"] = "Friends",
                ["FriendsTitle"] = "Friends",
                ["FriendsAccepted"] = "Accepted",
                ["FriendsPending"] = "Pending",
                ["FriendsDeclined"] = "Declined",
                ["FriendsBlocked"] = "Blocked",
                ["FriendsAll"] = "All",
                ["FriendsNone"] = "No friends yet",

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
                ["Register"] = "Register",


                #endregion

                #region UsersManagement 模組

                ["Users_Title"] = "Users Management",
                ["Users_Status"] = "Status",
                ["Users_Ban"] = "Ban",
                ["Users_CreateTime"] = "Createtime",
                ["Users_Enable"] = "Enable",
                ["Users_Disable"] = "Disable",
                ["Users_Yes"] = "Yes",
                ["Users_No"] = "No",
                ["Users_Name"] = "Name",
                ["Users_RoleText"] = "Role",
                ["Users_Email"] = "Email",
                ["Users_LastLoginTime"] = "Lastlogin",
                ["Users_Edit"] = "Edit",
                ["Users_Delete"] = "Delete",
                ["Users_Update"] = "Update",
                ["Users_Cancel"] = "Cancel",
                ["Users_SearchUserPlaceholder"] = "Please enter user name",
                ["Users_SearchDatePlaceholder"] = "Please enter YYYY-MM-DD",
                ["Users_Status_Disabled"] = "Disabled",
                ["Users_Status_Enabled"] = "Enabled",
                ["Users_Status_Banned"] = "Banned",
                ["Users_Role_Admin"] = "Admin",
                ["Users_Role_User"] = "User",


                #endregion

                #region Posts Management 模組

                ["Posts_Title"] = "Posts Management",
                ["Posts_Keyword"] = "Keyword",
                ["Posts_SearchKeywordPlaceholder"] = "Please enter keyword",
                ["Posts_Status"] = "Status",
                ["Posts_All"] = "All",
                ["Posts_Enable"] = "Enable",
                ["Posts_Disable"] = "Disable",
                ["Posts_CreateTime"] = "Create Time",
                ["Posts_Content"] = "Content",
                ["Posts_UserName"] = "User Name",
                ["Posts_ModifyTime"] = "Modify Time",
                ["Posts_Edit"] = "Edit",
                ["Posts_Delete"] = "Delete",
                ["Posts_Status_Normal"] = "Normal",
                ["Posts_Status_Hidden"] = "Hidden",
                ["Posts_Status_Deleted"] = "Deleted",
                ["Posts_Save"] = "Save",
                ["Posts_Cancel"] = "Cancel",
                ["Posts_ConfirmDelete"] = "Are you sure you want to delete this post?",
                ["Posts_DeleteError"] = "Delete failed",
                ["Posts_UpdateError"] = "Status update failed",
                ["Posts_LoadError"] = "Failed to load list",

                #endregion

                #region Reports Management 模組

                ["Reports_Title"] = "Reports Management",
                ["Reports_Reason"] = "Reason",
                ["Reports_Reporter"] = "Reporter",
                ["Reports_Type"] = "Type",
                ["Reports_Target"] = "Report Target",
                ["Reports_ModifyTime"] = "Modify Time",
                ["Reports_Status"] = "Status",
                ["Reports_Resolver"] = "Resolver",

                #endregion

                #region Overview 模組

                ["Overview_Title"] = "Admin Dashboard",
                ["Overview_WelcomeMsg"] = "Welcome to Matrix Admin Management System",
                ["Overview_TotalUsers"] = "Total Users",
                ["Overview_TotalPosts"] = "Total Posts",
                ["Overview_PendingReports"] = "Pending Reports",
                ["Overview_TodayActive"] = "Today Active",
                ["Overview_QuickActions"] = "Quick Actions",
                ["Overview_UserManagement"] = "User Management",
                ["Overview_PostManagement"] = "Post Management",
                ["Overview_ReportManagement"] = "Report Management",
                ["Overview_SystemStatus"] = "System Status",
                ["Overview_SystemUptime"] = "System Uptime",
                ["Overview_DatabaseConnection"] = "Database Connection",
                ["Overview_EmailService"] = "Email Service",
                ["Overview_Storage"] = "Storage",
                ["Overview_Status_Normal"] = "Normal",
                ["Overview_Status_InUse"] = "In Use",

                #endregion

                #region Config 模組

                ["Config_Title"] = "Settings",
                ["Config_Add_Admin"] = "Create Admin",
                ["Config_Filter"] = "List Filter",
                ["Config_ToolsState"] = "Tools",
                ["Config_WebLog"] = "Web logs",
                ["Config_AdminList"] = "Admin List",
                ["Config_AdminList_Keyword"] = "Search",
                ["Config_AdminList_KeywordPlaceholder"] = "Enter display name or user name.",
                ["Config_AdminList_UserName"] = "Account",
                ["Config_AdminList_DisplayName"] = "Display Name",
                ["Config_AdminList_Email"] = "Email",
                ["Config_AdminList_Admin"] = "Admin",
                ["Config_AdminList_SuperAdmin"] = "Super Admin",
                ["Config_AdminList_Status"] = "Account State",

                ["Config_LoginList"] = "Admin Activities",
                ["Config_LoginList_Keyword"] = "Search",
                ["Config_LoginList_IP"] = "IP",
                ["Config_LoginList_Role"] = "Permissions",
                ["Config_LoginList_Role_Admin"] = "Admin",
                ["Config_LoginList_Role_SuperAdmin"] = "Super Admin",
                ["Config_LoginList_Account"] = "Account",
                ["Config_LoginList_Browser"] = "Browser Info",
                ["Config_LoginList_PagePath"] = "Page Path",
                ["Config_LoginList_LoginTime"] = "Login Time",
                ["Config_LoginList_LogoutTime"] = "Logout Time",
                ["Config_LoginList_StartTime"] = "Event Start Time",
                ["Config_LoginList_DurationTime"] = "Duration (s)",
                ["Config_LoginList_ActionType"] = "Action Type",
                ["Config_LoginList_ActionType_VIEW"] = "VIEW",
                ["Config_LoginList_ActionType_CREATE"] = "CREATE",
                ["Config_LoginList_ActionType_UPDATE"] = "UPDATE",
                ["Config_LoginList_ActionType_DELETE"] = "DELETE",
                ["Config_LoginList_ActionType_ERROR"] = "ERROR",
                
                // 共用按鈕和操作
                ["Apply"] = "Apply",
                ["Clear"] = "Clear",

                #endregion

                // Validation error messages (from DTOs)
                #region User
                ["User_UserNameRequired"] = "User name is required.",
                ["User_UserNameLength1To50"] = "User name length must be between 1 and 50 characters.",
                ["User_EmailRequired"] = "Email is required.",
                ["User_EmailInvalid"] = "Please enter a valid email address.",
                ["User_EmailMaxLength100"] = "Email length must not exceed 100 characters.",
                ["User_CountryMaxLength100"] = "Country name length must not exceed 100 characters.",
                ["User_UserNameLength3To20"] = "User name length must be between 3 and 20 characters.",
                ["User_UserNameAllowedChars"] = "User name may only contain letters, numbers, and underscores.",
                ["User_EmailMaxLength30"] = "Email length must not exceed 30 characters.",
                ["User_PasswordRequired"] = "Password is required.",
                ["User_PasswordLength8To20"] = "Password length must be between 8 and 20 characters.",
                ["User_PasswordComplexity"] = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.",
                ["User_PasswordConfirmRequired"] = "Password confirmation is required.",
                ["User_PasswordsMustMatch"] = "Password confirmation must match the password.",
                ["User_GenderRange0To3"] = "Gender must be between 0 and 3.",
                ["User_RoleRange0To2"] = "Role level must be between 0 and 2.",
                ["User_DisplayNameMaxLength50"] = "Display name length must not exceed 50 characters.",
                ["User_DisplayNameLength1To50"] = "Display name length must be between 1 and 50 characters.",
                ["User_BioMaxLength300"] = "Bio length must not exceed 300 characters.",
                ["User_PrivacyRange0Or1"] = "Privacy must be 0 (public) or 1 (private).",
                ["User_UserNameAllowedCharsWithChinese"] = "User name may only contain letters, numbers, underscores, and Chinese characters.",

                // Avatar upload
                ["Avatar"] = "Avatar",
                ["ClickToUploadAvatar"] = "Click image to upload avatar",
                ["SupportedImageFormats"] = "Supports JPG, PNG, GIF (max 5MB)",
                #endregion

                #region Person
                ["Person_DisplayNameLength1To50"] = "Display name length must be between 1 and 50 characters.",
                ["Person_BioMaxLength300"] = "Bio length must not exceed 300 characters.",
                ["Person_PrivacyRange0To1"] = "Privacy value must be between 0 and 1.",
                ["Person_WalletAddressMaxLength100"] = "Wallet address length must not exceed 100 characters.",
                #endregion

                #region Notification
                ["Notification_TitleRequired"] = "Notification title is required.",
                ["Notification_TitleMaxLength100"] = "Notification title length must not exceed 100 characters.",
                ["Notification_ContentMaxLength500"] = "Notification content length must not exceed 500 characters.",
                #endregion

                #region Article
                ["Article_ContentRequired"] = "Article content is required.",
                ["Article_ContentMaxLength4000"] = "Article content length must not exceed 4000 characters.",
                ["Article_ContentLength1To4000"] = "Article content length must be between 1 and 4000 characters.",
                ["Article_IsPublicRange0Or1"] = "Article visibility must be 0 (public) or 1 (private).",
                #endregion

                #region Reply
                ["Reply_ArticleIdRequired"] = "Article ID is required.",
                ["Reply_ContentRequired"] = "Reply content is required.",
                ["Reply_ContentMaxLength1000"] = "Reply content length must not exceed 1000 characters.",
                ["Reply_ContentLength1To1000"] = "Reply content length must be between 1 and 1000 characters.",
                #endregion

                #region NFT
                ["NFT_OwnerIdRequired"] = "Owner ID is required.",
                ["NFT_FileNameRequired"] = "NFT name is required.",
                ["NFT_FileNameMaxLength255"] = "NFT name length must not exceed 255 characters.",
                ["NFT_FilePathRequired"] = "File path is required.",
                ["NFT_FilePathMaxLength2048"] = "File path length must not exceed 2048 characters.",
                ["NFT_CollectTimeRequired"] = "Collection time is required.",
                ["NFT_CurrencyRequired"] = "Currency is required.",
                ["NFT_CurrencyMaxLength10"] = "Currency length must not exceed 10 characters.",
                ["NFT_PriceMin0"] = "Price must be greater than or equal to 0.",
                ["NFT_MinPriceMin0"] = "Minimum price must be greater than or equal to 0.",
                ["NFT_MaxPriceMin0"] = "Maximum price must be greater than or equal to 0.",
                ["NFT_PageMin1"] = "Page must be greater than 0.",
                ["NFT_PageSizeRange1To100"] = "Page size must be between 1 and 100.",
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
