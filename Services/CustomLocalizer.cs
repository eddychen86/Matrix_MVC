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