using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    /// <summary>登入相關的 API</summary>
    [Route("api/login")]
    public class LoginController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomLocalizer _localizer;
        private readonly ILogger<LoginController> _logger;
        private readonly Matrix.Controllers.AuthController _authController;

        public LoginController(
            IUserService userService,
            ICustomLocalizer localizer,
            ILogger<LoginController> logger,
            Matrix.Controllers.AuthController authController
        )
        {
            _userService = userService;
            _localizer = localizer;
            _logger = logger;
            _authController = authController;
        }
        private static readonly string[] InvalidCredentialsError = ["Invalid user name or password."];

        /// <summary>處理登入 API 請求</summary>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel? model)
        {
            _logger.LogInformation("\n\n登入嘗試: {UserName}", model?.UserName);

            // 檢查模型和資料格式
            if (model == null || !ModelState.IsValid)
                return ValidationErrorResponse();

            // 驗證帳號密碼
            if (!await _userService.ValidateUserAsync(model.UserName, model.Password ?? string.Empty))
            {
                _logger.LogWarning("帳號密碼錯誤: {UserName}", model.UserName);
                return ApiError("帳號或密碼錯誤", new Dictionary<string, string[]> { { "", InvalidCredentialsError } });
            }

            _logger.LogInformation("\n\naccount: {0}\npassword: {1}\n\n", model.UserName, model.Password);

            // 取得用戶資料
            var userDto = await GetUserByIdentifierAsync(model.UserName);
            if (userDto == null)
            {
                _logger.LogWarning("找不到用戶: {UserName}", userDto);
                return ApiError("找不到用戶");
            }

            _logger.LogInformation("尋獲用戶: {0}", userDto);

            // 檢查帳號狀態
            var statusError = CheckUserStatus(userDto);
            if (statusError != null)
                return statusError;

            // 產生 JWT 並設定 Cookie
            var token = _authController.GenerateJwtToken(userDto.UserId, userDto.UserName, userDto.Role.ToString());
            _authController.SetAuthCookie(Response, token, model.RememberMe);

            return ApiSuccess(new { redirectUrl = "/home" }, "登入成功");
        }

        /// <summary>忘記密碼功能</summary>
        [HttpPost("api/login/forgot")]
        public IActionResult ForgotPassword([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse();

            // TODO: 實作忘記密碼功能
            return ApiError("忘記密碼功能尚未實作");
        }

        /// <summary>用帳號或信箱找用戶</summary>
        private async Task<UserDto?> GetUserByIdentifierAsync(string identifier)
        {
            return await _userService.GetUserByIdentifierAsync(identifier);
        }

        /// <summary>檢查用戶狀態是否正常</summary>
        private IActionResult? CheckUserStatus(UserDto userDto)
        {
            var errorMessage = userDto.Status switch
            {
                0 => _localizer["AccountNotVerified"].ToString(),
                2 => _localizer["AccountDisabled"].ToString(),
                _ => null
            };

            if (errorMessage != null)
            {
                _logger.LogWarning("帳號狀態異常: {UserName}, Status: {Status}", userDto.UserName, userDto.Status);
                return ApiError(errorMessage);
            }

            return null;
        }
    }
}