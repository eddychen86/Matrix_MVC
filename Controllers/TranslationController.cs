using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        // 靜態翻譯字典，完全脫離 .resx 依賴 - 包含所有原始 .resx 檔案內容
        private static readonly Dictionary<string, Dictionary<string, string>> AllTranslations = new()
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
                ["UsernameOrEmailExists"] = "用戶名或電子郵件已存在。",
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
                ["GuestBrowseMsg"] = "登入後可留言",
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
                ["UsernameOrEmailExists"] = "Username or email already exists.",

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
                ["GuestBrowseMsg"] = "Login",
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

                // 其他常用
                ["Title"] = "Matrix",
                ["Email"] = "Email",
                ["Password"] = "Password",
                ["ConfirmPassword"] = "Confirm Password",
                ["Register"] = "Register"
                
                #endregion
            }
        };

        [HttpGet("{culture}")]
        public IActionResult GetTranslations(string culture)
        {
            // 檢查是否為支援的語言
            if (!AllTranslations.ContainsKey(culture))
            {
                return BadRequest($"Unsupported culture: {culture}");
            }

            return Ok(AllTranslations[culture]);
        }

    }
}