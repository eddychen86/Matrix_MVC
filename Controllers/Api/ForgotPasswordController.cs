using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordController> _logger;

        public ForgotPasswordController(
            IUserService userService, 
            IEmailService emailService,
            ILogger<ForgotPasswordController> logger)
        {
            _userService = userService;
            _emailService = emailService;
            _logger = logger;
        }

        // [HttpGet("test-users")]
        // public async Task<IActionResult> TestUsers()
        // {
        //     // 僅用於調試 - 生產環境中應該移除
        //     try
        //     {
        //         var users = await _userService.GetUsersAsync(1, 10);
        //         var emailList = users.Users.Select(u => new { u.Email, u.UserName, u.Status }).ToList();
        //         return Ok(new { success = true, users = emailList });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Ok(new { success = false, error = ex.Message });
        //     }
        // }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "電子郵件格式不正確" });
                }

                _logger.LogInformation("Processing password reset request for email: {Email}", request.Email);
                
                // 生成臨時 token
                string temporaryToken = GenerateTemporaryToken();
                _logger.LogInformation("Generated temporary token for email: {Email}", request.Email);
                
                // 存儲加密的 token 到資料庫
                var tokenResult = await _userService.SetForgotPasswordTokenAsync(request.Email, temporaryToken);
                _logger.LogInformation("Token storage result for {Email}: {Result}", request.Email, tokenResult);
                
                if (!tokenResult)
                {
                    // 即使設置失敗（用戶不存在），仍然返回成功訊息以保護隱私
                    return Ok(new { success = true, message = "如果該電子郵件已註冊，將會收到密碼重置郵件" });
                }
                
                // 發送郵件 - 包含臨時 token
                _logger.LogInformation("Attempting to send email to {Email}", request.Email);
                
                try 
                {
                    var emailSent = await _emailService.SendPasswordResetEmailAsync(
                        request.Email, 
                        request.Email.Split('@')[0], // 使用 email 的用戶名部分
                        temporaryToken
                    );
                    
                    _logger.LogInformation("Email send result: {Result}", emailSent);
                    
                    if (!emailSent)
                    {
                        _logger.LogError("Email service returned false for {Email}", request.Email);
                        return StatusCode(500, new { success = false, message = "郵件發送失敗，請稍後再試" });
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Exception during email sending to {Email}", request.Email);
                    return StatusCode(500, new { success = false, message = "郵件發送異常，請稍後再試" });
                }
                
                _logger.LogInformation("Password reset email sent to {Email}", request.Email);
                return Ok(new { success = true, message = "密碼重置郵件已發送，請檢查您的郵箱" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset for {Email}", request.Email);
                return StatusCode(500, new { success = false, message = "系統錯誤，請稍後再試" });
            }
        }

        private string GenerateTemporaryToken()
        {
            // 生成符合密碼規則的臨時 token（作為臨時密碼使用）
            // 需要：8-20字符，大寫字母，小寫字母，數字，特殊字符
            
            var random = new Random();
            var token = new List<char>();
            
            // 定義字符集
            const string lowercase = "abcdefghijkmnpqrstuvwxyz";
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "2345678";
            const string specialChars = "!@#$%^&*";
            
            // 確保包含每種必需的字符類型
            token.Add(lowercase[random.Next(lowercase.Length)]);     // 小寫字母
            token.Add(uppercase[random.Next(uppercase.Length)]);     // 大寫字母
            token.Add(digits[random.Next(digits.Length)]);           // 數字
            token.Add(specialChars[random.Next(specialChars.Length)]); // 特殊字符
            
            // 填充剩餘字符到8位（所有字符集合）
            const string allChars = lowercase + uppercase + digits + specialChars;
            for (int i = 4; i < 8; i++)
            {
                token.Add(allChars[random.Next(allChars.Length)]);
            }
            
            // 打亂順序
            for (int i = token.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (token[i], token[j]) = (token[j], token[i]);
            }
            
            return new string(token.ToArray());
        }
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        public string Email { get; set; } = string.Empty;
    }
}