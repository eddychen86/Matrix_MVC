using Microsoft.AspNetCore.Mvc;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

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
        private readonly IAdminActivityService _adminActivityService;

        public LoginController(
            IUserService userService,
            IUserRepository userRepository,
            ICustomLocalizer localizer,
            ILogger<LoginController> logger,
            Matrix.Controllers.AuthController authController,
            IAuthorizationService authorizationService,
            IAdminActivityService adminActivityService
        )
        {
            _userService = userService;
            _userRepository = userRepository;
            _localizer = localizer;
            _logger = logger;
            _authController = authController;
            _authorizationService = authorizationService;
            _adminActivityService = adminActivityService;
        }

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

            // 更新最後登入時間
            var updateLoginTimeResult = await _userService.UpdateLastLoginTimeAsync(userDto.UserId);
            if (!updateLoginTimeResult)
            {
                _logger.LogWarning("Failed to update last login time for user {UserName}", userDto.UserName);
                // 不要因為更新登入時間失敗而阻止登入，只記錄警告
            }
            else
            {
                _logger.LogInformation("Updated last login time for user {UserName}", userDto.UserName);
            }

            // TODO: 檢查是否需要強制修改密碼 - 需要先創建資料庫遷移
            // var userEntity = await _userRepository.GetByIdAsync(userDto.UserId);
            // if (userEntity?.ForcePasswordChange == true)
            // {
            //     // 仍然需要設定 JWT 和 Cookie 以便用戶能夠修改密碼
            //     var token = _authController.GenerateJwtToken(userDto);
            //     _authController.SetAuthCookie(Response, token, model.RememberMe);
            //     
            //     _logger.LogInformation("User {UserName} requires password change", userDto.UserName);
            //     
            //     // 返回特殊響應表示需要強制修改密碼
            //     return ApiSuccess(new { 
            //         forcePasswordChange = true,
            //         redirectUrl = userDto.Role >= 1 ? "/dashboard/overview/index" : "/home/index"
            //     }, "需要修改密碼");
            // }

            // 產生 JWT 並設定 Cookie
            var normalToken = _authController.GenerateJwtToken(userDto);
            _logger.LogInformation("Generated JWT token for user {UserName}, token length: {TokenLength}", userDto.UserName, normalToken.Length);
            
            _authController.SetAuthCookie(Response, normalToken, model.RememberMe);
            _logger.LogInformation("Set auth cookie for user {UserName}, RememberMe: {RememberMe}", userDto.UserName, model.RememberMe);

            // 根據用戶角色決定跳轉目標
            var redirectUrl = userDto.Role >= 1 ? "/dashboard/overview/index" : "/home/index";
            
            _logger.LogInformation("Login successful for user {UserName} (Role: {Role}), redirecting to: {RedirectUrl}", 
                userDto.UserName, userDto.Role, redirectUrl);

            // 為管理員用戶記錄登入活動
            if (userDto.Role >= 1)
            {
                try
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                    
                    await _adminActivityService.LogLoginAsync(
                        userDto.UserId, 
                        userDto.UserName ?? "Unknown", 
                        userDto.Role, 
                        ipAddress, 
                        userAgent
                    );
                    
                    _logger.LogInformation("Admin login activity recorded for user {UserName}", userDto.UserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to record admin login activity for user {UserName}", userDto.UserName);
                    // 不要因為活動記錄失敗而影響登入流程
                }
            }

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


        /// <summary>用帳號或信箱找用戶</summary>
        private async Task<UserDto?> GetUserByIdentifierAsync(string Name_Or_Email)
        {
            _logger.LogInformation("Calling UserService.GetUserByIdentifierAsync with: {Identifier}", Name_Or_Email);

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