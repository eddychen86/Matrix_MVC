using Microsoft.AspNetCore.Http;

namespace Matrix.Services
{
    public interface ICustomLocalizer
    {
        string this[string key] { get; }
    }

    public class CustomLocalizer : ICustomLocalizer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // 靜態翻譯字典，與 TranslationController 保持一致
        private static readonly Dictionary<string, Dictionary<string, string>> AllTranslations = new()
        {
            ["zh-TW"] = new Dictionary<string, string>
            {
                // === Auth 模組 ===
                ["LoginTitle"] = "登入",
                ["RegisterTitle"] = "註冊",
                ["ForgotPasswordTitle"] = "忘記密碼",
                ["WelcomeMsg"] = "加入區塊鏈社群",
                ["ForgotPasswordMsg"] = "請輸入您的電子郵件地址以重置密碼。",
                ["NewHereMsg"] = "新來的？",
                ["hasAccountMsg"] = "已經有帳號？",
                ["UserNameLabel"] = "用戶名",
                ["EmailLabel"] = "電子郵件",
                ["PasswordLabel"] = "密碼",
                ["ConfirmPasswordLabel"] = "確認密碼",
                ["RememberMeLabel"] = "記住我？",
                ["ForgotPasswordLabel"] = "忘記密碼？",
                ["SubmitBtn"] = "送出",
                ["UserNamePlaceholder"] = "請輸入用戶名",
                ["EmailPlaceholder"] = "請輸入電子郵件",
                ["PasswordPlaceholder"] = "請輸入密碼",
                ["ConfirmPasswordPlaceholder"] = "請確認密碼",
                
                // === Common 模組 ===
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
                ["Comment"] = "留言",
                ["Praise"] = "讚",
                ["Follow"] = "追蹤",
                ["Collect"] = "收藏",
                ["Share"] = "分享",
                ["GuestBrowseMsg"] = "登入後可留言",
                ["GuestAccount"] = "訪客",
                
                // === Menu 模組 ===
                ["Matrix"] = "Matrix",
                ["Login"] = "登入",
                ["Search"] = "搜尋",
                ["Notify"] = "通知",
                ["Follows"] = "追蹤",
                ["Collects"] = "收藏",
                ["Language"] = "繁體中文",
                ["HideBar"] = "隱藏側欄",
                ["LogOut"] = "登出",
                
                // === Home 模組 ===
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
                
                // === Profile 模組 ===
                ["ProfileName"] = "個人資料名稱",
                ["ProfileDescription"] = "個人資料描述",
                ["EditProfile"] = "編輯個人資料",
                ["SaveProfile"] = "儲存個人資料",
                
                // === Admin 模組 ===
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
                ["EmailWelcomeSubtitle"] = "Welcome to the lighthouse.",
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
            },
            
            ["en-US"] = new Dictionary<string, string>
            {
                // === Auth 模組 ===
                ["LoginTitle"] = "Login",
                ["RegisterTitle"] = "Register",
                ["ForgotPasswordTitle"] = "Forgot Password",
                ["WelcomeMsg"] = "Join the blockchain community",
                ["ForgotPasswordMsg"] = "Please enter your email address to reset your password.",
                ["NewHereMsg"] = "New here? Let ",
                ["hasAccountMsg"] = "Already have an account?",
                ["UserNameLabel"] = "User Name",
                ["EmailLabel"] = "Email Address",
                ["PasswordLabel"] = "Password",
                ["ConfirmPasswordLabel"] = "Confirm Password",
                ["RememberMeLabel"] = "Remember me",
                ["ForgotPasswordLabel"] = "Forgot password?",
                ["SubmitBtn"] = "Submit",
                ["UserNamePlaceholder"] = "Enter User Name",
                ["EmailPlaceholder"] = "Enter Email",
                ["PasswordPlaceholder"] = "Enter Password",
                ["ConfirmPasswordPlaceholder"] = "Confirm Password",
                
                // === Common 模組 ===
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
                ["Comment"] = "Comment",
                ["Praise"] = "Praise",
                ["Follow"] = "Follow",
                ["Collect"] = "Collect",
                ["Share"] = "Share",
                ["GuestBrowseMsg"] = "Login",
                ["GuestAccount"] = "Guest",
                
                // === Menu 模組 ===
                ["Matrix"] = "Matrix",
                ["Login"] = "Login",
                ["Search"] = "Search",
                ["Notify"] = "Notify",
                ["Follows"] = "Follows",
                ["Collects"] = "Collects",
                ["Language"] = "English",
                ["HideBar"] = "Hide bar",
                ["LogOut"] = "Log out",
                
                // === Home 模組 ===
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
                
                // === Profile 模組 ===
                ["ProfileName"] = "Profile Name",
                ["ProfileDescription"] = "Profile Description",
                ["EditProfile"] = "Edit Profile",
                ["SaveProfile"] = "Save Profile",
                
                // === Admin 模組 ===
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
                ["EmailWelcomeSubtitle"] = "Welcome to the lighthouse.",
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
            }
        };

        public CustomLocalizer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string this[string key]
        {
            get
            {
                // 獲取當前語言，預設為繁體中文
                var currentCulture = GetCurrentCulture();
                
                // 如果找到翻譯則返回，否則返回 key 本身
                if (AllTranslations.ContainsKey(currentCulture) && 
                    AllTranslations[currentCulture].ContainsKey(key))
                {
                    return AllTranslations[currentCulture][key];
                }
                
                // 找不到翻譯時回退到中文
                if (currentCulture != "zh-TW" && 
                    AllTranslations["zh-TW"].ContainsKey(key))
                {
                    return AllTranslations["zh-TW"][key];
                }
                
                // 都找不到則返回 key
                return key;
            }
        }

        private string GetCurrentCulture()
        {
            // 從 Cookie 中獲取語言設定
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies.ContainsKey(".AspNetCore.Culture") == true)
            {
                var cultureCookie = httpContext.Request.Cookies[".AspNetCore.Culture"];
                if (!string.IsNullOrEmpty(cultureCookie) && cultureCookie.Contains("c="))
                {
                    var culture = cultureCookie.Split('|')[0].Replace("c=", "");
                    return culture;
                }
            }
            
            // 預設返回繁體中文
            return "zh-TW";
        }
    }
}