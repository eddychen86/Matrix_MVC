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
        private readonly IUserRegistrationService _registrationService;
        private readonly ILogger<RegisterController> _logger;
        private readonly IEmailService _emailService;
        private readonly ICustomLocalizer _localizer;

        public RegisterController(
            IUserRegistrationService registrationService,
            ILogger<RegisterController> logger,
            IEmailService emailService,
            ICustomLocalizer localizer
        )
        {
            _registrationService = registrationService;
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
            if (model == null)
            {
                return ApiError(_localizer["Error"], new Dictionary<string, string[]> { { "", [_localizer["Error"]] } });
            }

            // 使用註冊服務進行驗證和建立用戶（一般用戶角色 = 0）
            var (userId, errors) = await _registrationService.RegisterUserAsync(model, role: 0);

            if (userId == null)
            {
                // 將錯誤映射到前端欄位
                var fieldErrors = _registrationService.MapServiceErrorsToFieldErrors(errors);
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