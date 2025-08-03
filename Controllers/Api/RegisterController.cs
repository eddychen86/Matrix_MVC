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
        private readonly IEmailService _emailService;
        private readonly ICustomLocalizer _localizer;

        public RegisterController(
            IUserService userService,
            ILogger<RegisterController> logger,
            IEmailService emailService,
            ICustomLocalizer localizer
        )
        {
            _userService = userService;
            _logger = logger;
            _emailService = emailService;
            _localizer = localizer;
        }

        /// <summary>
        /// 處理註冊 API 請求
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel? model)
        {
            _logger.LogInformation("\n\n註冊嘗試: {UserName}\n", model?.UserName);

            // 手動驗證前端基本規則
            var validationErrors = new Dictionary<string, string[]>();
            
            if (model == null)
            {
                return ApiError(_localizer["Error"], new Dictionary<string, string[]> { { "", [_localizer["Error"]] } });
            }
            
            // 用戶名驗證
            if (string.IsNullOrWhiteSpace(model.UserName))
                validationErrors["UserName"] = [_localizer["UserNameInvalid"]];
            else if (model.UserName.Length < 3 || model.UserName.Length > 20)
                validationErrors["UserName"] = [_localizer["UserNameFormatError"]];
                
            // 郵件驗證
            if (string.IsNullOrWhiteSpace(model.Email))
                validationErrors["Email"] = [_localizer["EmailRequired"]];
            else if (!model.Email.Contains('@') || !model.Email.Contains('.'))
                validationErrors["Email"] = [_localizer["EmailInvalid"]];
            else if (model.Email.Length > 30)
                validationErrors["Email"] = [_localizer["EmailFormatError"]];
                
            // 密碼驗證
            if (string.IsNullOrWhiteSpace(model.Password))
                validationErrors["Password"] = [_localizer["PasswordInvalid"]];
            else if (model.Password.Length < 8 || model.Password.Length > 20)
                validationErrors["Password"] = [_localizer["PasswordFormatError"]];
                
            // 確認密碼驗證
            if (string.IsNullOrWhiteSpace(model.PasswordConfirm))
                validationErrors["PasswordConfirm"] = [_localizer["PasswordConfirmRequired"]];
            else if (model.Password != model.PasswordConfirm)
                validationErrors["PasswordConfirm"] = [_localizer["PasswordCompareError"]];
                
            // 如果有前端驗證錯誤，直接返回
            if (validationErrors.Count > 0)
            {
                return ApiError(_localizer["Error"], validationErrors);
            }

            var createUserDto = new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email ?? string.Empty,
                Password = model.Password,
                PasswordConfirm = model.PasswordConfirm
            };

            var (userId, errors) = await _userService.CreateUserAsync(createUserDto);

            _logger.LogInformation(
                "\n\nError:\n{errors}\n\n", errors
            );

            if (userId == null)
            {
                _logger.LogWarning("\n註冊失敗: {Errors}\n", string.Join(", ", errors));

                // 將 UserService 的驗證錯誤映射到對應的 ViewModel 欄位
                var fieldErrors = new Dictionary<string, string[]>();
                foreach (var error in errors)
                {
                    // 根據錯誤內容映射到正確的欄位，使用多語系訊息
                    if (error.Contains("用戶名") || error.Contains("UserName") || error.Contains("username"))
                    {
                        if (error.Contains("長度") || error.Contains("字"))
                            fieldErrors["UserName"] = [_localizer["UserNameFormatError"]];
                        else if (error.Contains("已被使用") || error.Contains("exists"))
                            fieldErrors["UserName"] = [_localizer["UsernameExists"]];
                        else
                            fieldErrors["UserName"] = [_localizer["UserNameInvalid"]];
                    }
                    else if (error.Contains("郵件") || error.Contains("Email") || error.Contains("email"))
                    {
                        if (error.Contains("格式") || error.Contains("invalid"))
                            fieldErrors["Email"] = [_localizer["EmailInvalid"]];
                        else if (error.Contains("已被使用") || error.Contains("exists"))
                            fieldErrors["Email"] = [_localizer["EmailExists"]];
                        else if (error.Contains("必填") || error.Contains("required"))
                            fieldErrors["Email"] = [_localizer["EmailRequired"]];
                        else
                            fieldErrors["Email"] = [_localizer["EmailFormatError"]];
                    }
                    else if (error.Contains("密碼") || error.Contains("Password") || error.Contains("password"))
                    {
                        if (error.Contains("確認") || error.Contains("confirm") || error.Contains("不相符") || error.Contains("match"))
                            fieldErrors["PasswordConfirm"] = [_localizer["PasswordCompareError"]];
                        else if (error.Contains("格式") || error.Contains("大寫") || error.Contains("小寫") || error.Contains("數字") || error.Contains("特殊"))
                            fieldErrors["Password"] = [_localizer["PasswordFormatError"]];
                        else
                            fieldErrors["Password"] = [_localizer["PasswordInvalid"]];
                    }
                    else
                    {
                        // 一般錯誤
                        fieldErrors[""] = [error];
                    }
                }

                return ApiError(_localizer["Error"], fieldErrors);
            }

            try
            {
                await SendConfirmationEmail(model, userId.Value.ToString());
                return ApiSuccess(new
                {
                    redirectUrl = "/login",
                    emailSent = true
                }, _localizer["ConfirmEmailSent"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\n\n註冊成功但發送確認信失敗\n\n");
                return ApiSuccess(new
                {
                    redirectUrl = "/login",
                    emailSent = false
                }, _localizer["SendConfirmEmailError"]);
            }
        }

        /// <summary>發送確認信</summary>
        private async Task SendConfirmationEmail(RegisterViewModel model, string userId)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Email))
            {
                _logger.LogWarning("SendConfirmationEmail: model.UserName or model.Email is null or empty.");
                return;
            }

            _logger.LogInformation("\n\nHTTP 協定：{0}\nHost：{1}\nUser Id：{2}\n\n", Request.Scheme, Request.Host, userId);

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/confirm/{userId}";
            string emailBody = GenerateConfirmationEmailBody(model.UserName, confirmationLink);

            await _emailService.SendEmailAsync(
                model.Email,
                model.UserName,
                _localizer["WelcomeRegisterConfirmEmail"],
                emailBody
            );

            _logger.LogInformation("確認信已發送至: {Email}", model.Email);
        }

        /// <summary>生成確認信內容</summary>
        private string GenerateConfirmationEmailBody(string userName, string confirmationLink)
        {
            var greeting = string.Format(_localizer["EmailGreeting"], userName);
            
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #082032 0%, #2C394B 100%); padding: 40px; text-align: center;'>
                        <h1 style='color: #FF4C29; margin: 0;'>{_localizer["EmailWelcomeTitle"]}</h1>
                        <p style='color: #FFFFFF; margin: 10px 0 0 0;'>{_localizer["EmailWelcomeSubtitle"]}</p>
                    </div>
                    <div style='padding: 40px; background-color: #f9f9f9;'>
                        <h2 style='color: #082032;'>{greeting}</h2>
                        <p style='color: #334756; line-height: 1.6;'>
                            {_localizer["EmailMainContent"]}
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='display: inline-block; padding: 15px 30px; background-color: #FF4C29; color: white; text-decoration: none; border-radius: 1000px; font-weight: bold;'>
                                {_localizer["EmailConfirmButton"]}
                            </a>
                        </div>
                        <p style='color: #334756; font-size: 14px;'>
                            {_localizer["EmailAlternativeText"]}<br>
                            <a href='{confirmationLink}' style='color: #FF4C29;'>{confirmationLink}</a>
                        </p>
                        <p style='color: #334756; font-size: 12px; margin-top: 30px; font-style: italic;'>
                            {_localizer["EmailFooterText"]}<br>
                            {_localizer["EmailBrandMotto"]}
                        </p>
                    </div>
                </div>";
        }

        // [HttpGet("test-email")]
        // public async Task<IActionResult> TestEmail()
        // {
        //     var model = new RegisterViewModel
        //     {
        //         UserName = "eddychen",
        //         Email = "eddychen101020@gmail.com",
        //     };
        //     var userId = Guid.NewGuid().ToString();

        //     try
        //     {
        //         await SendConfirmationEmail(model, userId);
        //         return ApiSuccess(new { emailSent = true }, "測試郵件已成功發送。");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "發送測試郵件失敗");
        //         return ApiError("發送測試郵件失敗", new Dictionary<string, string[]> { { "General", new[] { ex.Message } } });
        //     }
        // }
    }
}