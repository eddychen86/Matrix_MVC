using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Microsoft.Extensions.Localization;

namespace Matrix.Controllers
{
    public class LoginController(
        IUserService _userService,
        IStringLocalizer<LoginController> _localizer,
        ILogger<LoginController> _logger,
        AuthController _authController
    ) : Controller
    {
        private static readonly string[] InvalidCredentialsError = ["Invalid user name or password."];

        /// <summary>顯示登入頁面</summary>
        [HttpGet, Route("/login")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }

        /// <summary>處理登入 API 請求</summary>
        [HttpPost, Route("/api/login")]
        public async Task<IActionResult> LoginApi([FromBody] LoginViewModel? model)
        {
            _logger.LogInformation("\n\n登入嘗試: {UserName}\n\n", model?.UserName);

            // 檢查模型和資料格式
            if (model == null || !ModelState.IsValid)
                return ValidationErrorResponse();

            // 驗證帳號密碼
            if (!await _userService.ValidateUserAsync(model.UserName, model.Password ?? string.Empty))
            {
                _logger.LogWarning("\n\n帳號密碼錯誤: {UserName}, {Password}\n\n", model.UserName, model.Password);
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "", InvalidCredentialsError } } });
            }

            // 取得用戶資料
            var userDto = await GetUserByIdentifierAsync(model.UserName);
            if (userDto == null)
                return Json(new { success = false, errors = new { Error = "\n\n找不到用戶\n\n" } });

            // 檢查帳號狀態
            var statusError = CheckUserStatus(userDto);
            if (statusError != null)
                return statusError;

            // 產生 JWT 並設定 Cookie
            var token = _authController.GenerateJwtToken(userDto.UserId, userDto.UserName, userDto.Role.ToString());
            _authController.SetAuthCookie(token, model.RememberMe);

            return Json(new { success = true, redirectUrl = "/home/index" });
        }

        /// <summary>忘記密碼功能</summary>
        [HttpPost, Route("api/login/forgot")]
        public ActionResult ForgotPassword(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/Login.cshtml", model);

            // TODO: 實作忘記密碼功能
            ModelState.AddModelError("", "忘記密碼功能尚未實作");
            return View("~/Views/Auth/Login.cshtml", model);
        }

        #region 私人方法

        /// <summary>回傳驗證錯誤的統一格式</summary>
        private IActionResult ValidationErrorResponse()
        {
            var errors = ModelState
                .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return Json(new { success = false, errors });
        }

        /// <summary>用帳號或信箱找用戶</summary>
        private async Task<UserDto?> GetUserByIdentifierAsync(string identifier)
        {
            return await _userService.GetUserByEmailAsync(identifier) ?? 
                   await _userService.GetUserByUsernameAsync(identifier);
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
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "", [errorMessage] } } });
            }

            return null;
        }

        #endregion
    }
}