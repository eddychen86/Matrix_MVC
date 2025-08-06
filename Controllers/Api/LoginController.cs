using Microsoft.AspNetCore.Mvc;
using Matrix.Repository.Interfaces;

namespace Matrix.Controllers.Api
{
    /// <summary>登入相關的 API</summary>
    [Route("api/login")]
    public class LoginController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ICustomLocalizer _localizer;
        private readonly ILogger<LoginController> _logger;
        private readonly Matrix.Controllers.AuthController _authController;
        private readonly IAuthorizationService _authorizationService;

        public LoginController(
            IUserService userService,
            IUserRepository userRepository,
            ICustomLocalizer localizer,
            ILogger<LoginController> logger,
            Matrix.Controllers.AuthController authController,
            IAuthorizationService authorizationService
        )
        {
            _userService = userService;
            _userRepository = userRepository;
            _localizer = localizer;
            _logger = logger;
            _authController = authController;
            _authorizationService = authorizationService;
        }
        private static readonly string[] InvalidCredentialsError = ["Invalid user name or password."];

        /// <summary>處理登入 API 請求</summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginViewModel? model)
        {
            _logger.LogInformation("\n\n登入嘗試: {UserName}", model?.UserName);

            // 第一層：基本格式驗證（顯示在各自輸入框下方）
            var validationErrors = new Dictionary<string, string[]>();
            
            if (model == null)
            {
                return ApiError(_localizer["Error"], new Dictionary<string, string[]> { { "", [_localizer["Error"]] } });
            }
            
            // 用戶名驗證
            if (string.IsNullOrWhiteSpace(model.UserName))
                validationErrors["UserName"] = [_localizer["UserNameInvalid"]];
                
            // 密碼驗證
            if (string.IsNullOrWhiteSpace(model.Password))
                validationErrors["Password"] = [_localizer["PasswordInvalid"]];
                
            // 如果有基本格式錯誤，直接返回
            if (validationErrors.Count > 0)
            {
                return ApiError(_localizer["Error"], validationErrors);
            }

            // 驗證帳號密碼
            if (!await _userService.ValidateUserAsync(model.UserName, model.Password ?? string.Empty))
            {
                _logger.LogWarning("帳號密碼錯誤: {UserName}", model.UserName);
                return ApiError(_localizer["AccountLoginError"], new Dictionary<string, string[]> { { "AccountLoginError", [_localizer["AccountLoginError"]] } });
            }

            _logger.LogInformation("\n\naccount: {0}\npassword: {1}\n\n", model.UserName, model.Password);

            // 取得用戶資料
            var userDto = await GetUserByIdentifierAsync(model.UserName);
            if (userDto == null)
            {
                _logger.LogWarning("找不到用戶: {UserName}", model.UserName);
                return ApiError(_localizer["AccountLoginError"], new Dictionary<string, string[]> { { "AccountLoginError", [_localizer["AccountLoginError"]] } });
            }

            _logger.LogInformation("尋獲用戶: {0}", userDto);

            // 檢查帳號狀態
            var statusError = await CheckUserStatusAsync(userDto.UserId);
            if (statusError != null)
                return statusError;

            _logger.LogInformation("帳號狀態：{0}", statusError);

            // 產生 JWT 並設定 Cookie
            var token = _authController.GenerateJwtToken(userDto);
            _logger.LogInformation("Generated JWT token for user {UserName}, token length: {TokenLength}", userDto.UserName, token.Length);
            
            _authController.SetAuthCookie(Response, token, model.RememberMe);
            _logger.LogInformation("Set auth cookie for user {UserName}, RememberMe: {RememberMe}", userDto.UserName, model.RememberMe);

            // 根據用戶角色決定跳轉目標
            var redirectUrl = userDto.Role >= 1 ? "/dashboard/overview/index" : "/home/index";
            
            _logger.LogInformation("Login successful for user {UserName} (Role: {Role}), redirecting to: {RedirectUrl}", 
                userDto.UserName, userDto.Role, redirectUrl);

            // 確保返回正確的 API 格式
            var response = new { redirectUrl };
            _logger.LogInformation("API Response: {Response}", System.Text.Json.JsonSerializer.Serialize(response));

            return ApiSuccess(response, _localizer["Success"]);
        }

        /// <summary>測試 Cookie 設定</summary>
        [HttpPost("test-cookie")]
        [IgnoreAntiforgeryToken]
        public IActionResult TestCookie()
        {
            // 設定一個測試 Cookie
            Response.Cookies.Append("TestCookie", "TestValue", new CookieOptions
            {
                HttpOnly = false, // 允許 JavaScript 存取以方便測試
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(10)
            });

            _logger.LogInformation("Test cookie set successfully");

            return ApiSuccess(new { message = "Test cookie set", testTime = DateTime.UtcNow });
        }

        /// <summary>測試認證狀態</summary>
        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            var authInfo = HttpContext.GetAuthInfo();
            
            _logger.LogInformation("=== Auth Test ===");
            _logger.LogInformation("IsAuthenticated: {IsAuthenticated}", authInfo.IsAuthenticated);
            _logger.LogInformation("UserId: {UserId}", authInfo.UserId);
            _logger.LogInformation("UserName: {UserName}", authInfo.UserName);
            _logger.LogInformation("Role: {Role}", authInfo.Role);
            
            return ApiSuccess(new 
            { 
                authInfo.IsAuthenticated,
                authInfo.UserId,
                authInfo.UserName,
                authInfo.Role,
                contextItems = HttpContext.Items.Keys.ToArray()
            });
        }

        /// <summary>忘記密碼功能</summary>
        [HttpPost("api/login/forgot")]
        public IActionResult ForgotPassword([FromBody] LoginViewModel model)
        {
            // 基本格式驗證
            var validationErrors = new Dictionary<string, string[]>();
            
            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
                validationErrors["UserName"] = [_localizer["UserNameInvalid"]];
                
            if (validationErrors.Count > 0)
            {
                return ApiError(_localizer["Error"], validationErrors);
            }

            // TODO: 實作忘記密碼功能
            return ApiError(_localizer["ForgotPasswordMsg"]);
        }

        /// <summary>用帳號或信箱找用戶</summary>
        private async Task<UserDto?> GetUserByIdentifierAsync(string Name_Or_Email)
        {
            _logger.LogInformation("Calling UserService.GetUserByIdentifierAsync with: {Identifier}", Name_Or_Email);

            // 先直接測試 Repository 層
            var userFromRepo = await _userRepository.GetByIdentifierAsync(Name_Or_Email);
            _logger.LogInformation("Repository result: User found={UserFound}, Person found={PersonFound}",
                userFromRepo != null, userFromRepo?.Person != null);

            // 再調用 Service 層
            var result = await _userService.GetUserByIdentifierAsync(Name_Or_Email);
            _logger.LogInformation("Service result: {ServiceResult}", result != null ? "Found" : "Null");

            return result;
        }

        /// <summary>檢查用戶狀態是否正常</summary>
        private async Task<IActionResult?> CheckUserStatusAsync(Guid userId)
        {
            var statusResult = await _authorizationService.CheckUserStatusAsync(userId);
            if (!statusResult.IsValid)
            {
                _logger.LogWarning("帳號狀態異常: {UserId}, StatusCode: {StatusCode}", userId, statusResult.StatusCode);
                return ApiError(statusResult.ErrorMessage ?? _localizer["AccountLoginError"], 
                    new Dictionary<string, string[]> { { "AccountLoginError", [statusResult.ErrorMessage ?? _localizer["AccountLoginError"]] } });
            }

            return null;
        }

        /// <summary>測試 Repository 查詢使用者</summary>
        // [HttpGet("test-repository/{identifier}")]
        // public async Task<IActionResult> TestRepository(string identifier)
        // {
        //     _logger.LogInformation("Testing UserRepository.GetByIdentifierAsync with identifier: {Identifier}", identifier);

        //     var user = await _userRepository.GetByIdentifierAsync(identifier);

        //     if (user == null)
        //     {
        //         _logger.LogWarning("UserRepository.GetByIdentifierAsync returned null for identifier: {Identifier}", identifier);
        //         return ApiError($"找不到使用者: {identifier}");
        //     }

        //     _logger.LogInformation("UserRepository.GetByIdentifierAsync found user - UserId: {UserId}, UserName: {UserName}, Email: {Email}, Person is null: {PersonIsNull}", 
        //         user.UserId, user.UserName, user.Email, user.Person == null);

        //     return ApiSuccess(new 
        //     { 
        //         UserId = user.UserId,
        //         UserName = user.UserName,
        //         Email = user.Email,
        //         PersonIsNull = user.Person == null,
        //         CreateTime = user.CreateTime,
        //         Status = user.Status
        //     }, "找到使用者");
        // }
    }
}