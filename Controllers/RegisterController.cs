using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class RegisterController(
        IUserService _userService,
        ILogger<RegisterController> _logger,
        AuthController _authController
    ) : Controller
    {
        // TODO:顯示註冊頁面
        [HttpGet, Route("/register")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        // TODO:處理註冊 API 請求
        [HttpPost, Route("/api/register")]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterViewModel? model)
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
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "UserName", result.Errors.ToArray() } } });
            }
            else
            {
                // 呼叫 ValidMail 發送確認信
                try
                {
                    // 這裡可以直接呼叫 AuthController 的方法，或是使用 HTTP 請求
                    // 方案1: 直接注入 AuthController
                    await _authController.SendConfirmationEmail(model);

                    // 方案2: 回傳成功並提示前端發送確認信
                    return Json(new
                    {
                        success = true,
                        needEmailConfirmation = true,
                        message = "註冊成功！正在發送確認信到您的郵箱...",
                        email = model.Email
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "註冊成功但發送確認信失敗");
                    return Json(new
                    {
                        success = true,
                        message = "註冊成功！但確認信發送失敗，請稍後手動重新發送。",
                        redirectUrl = "/login"
                    });
                }
            }
        }

        // TODO:回傳驗證錯誤的統一格式
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
    }
}