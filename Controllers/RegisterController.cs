using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Matrix.DTOs;

namespace Matrix.Controllers
{
    public class RegisterController(IUserService _userService, ILogger<RegisterController> _logger) : Controller
    {
        /// <summary>顯示註冊頁面</summary>
        [HttpGet, Route("/register")]
        public ActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        /// <summary>處理註冊 API 請求</summary>
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

            // 註冊成功
            return Json(new { success = true, redirectUrl = Url.Action("Auth", "login") });
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

        #endregion
    }
}