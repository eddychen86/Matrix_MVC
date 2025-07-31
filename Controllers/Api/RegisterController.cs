using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Matrix.DTOs;

namespace Matrix.Controllers.Api
{
    /// <summary>註冊相關的 API</summary>
    [Route("api/register")]
    public class RegisterController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<RegisterController> _logger;
        private readonly Matrix.Controllers.Api.AuthController _authController;

        public RegisterController(
            IUserService userService,
            ILogger<RegisterController> logger,
            Matrix.Controllers.Api.AuthController authController
        )
        {
            _userService = userService;
            _logger = logger;
            _authController = authController;
        }

        /// <summary>處理註冊 API 請求</summary>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel? model)
        {
            _logger.LogInformation("註冊嘗試: {UserName}", model?.UserName);

            // 檢查資料格式是否正確
            if (model == null || !ModelState.IsValid)
                return ValidationErrorResponse();

            // 轉換資料格式並建立用戶
            var createUserDto = new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email ?? "example@mail.com", // 暫時預設值
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm ?? model.Password
            };

            var result = await _userService.CreateUserAsync(createUserDto);

            // 註冊失敗就回傳錯誤
            if (result.UserId == null)
            {
                _logger.LogWarning("註冊失敗: {Errors}", string.Join(", ", result.Errors));
                return ApiError("註冊失敗", new Dictionary<string, string[]> { { "UserName", result.Errors.ToArray() } });
            }
            else
            {
                // 註冊成功，自動發送確認信
                try
                {
                    await _authController.SendConfirmationEmail(model);
                    
                    return ApiSuccess(new {
                        redirectUrl = "/login",
                        emailSent = true
                    }, "註冊成功！確認信已發送到您的郵箱，請查收並點擊確認連結。");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "註冊成功但發送確認信失敗");
                    return ApiSuccess(new { 
                        redirectUrl = "/login",
                        emailSent = false
                    }, "註冊成功！但確認信發送失敗，請稍後手動重新發送。");
                }
            }
        }
    }
}