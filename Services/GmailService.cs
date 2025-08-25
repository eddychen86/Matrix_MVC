using Microsoft.Extensions.Options;
using Matrix.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Matrix.Services
{
    public class GmailService : IEmailService
    {
        private readonly GoogleSmtpDTOs _settings;
        private readonly ILogger<GmailService> _logger;

        public GmailService(IOptions<GoogleSmtpDTOs> settings, ILogger<GmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation("\n\n開始使用 MailKit 發送郵件到: {Email}\n", toEmail);

                // 1. 建立郵件內容 (MimeKit)
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

                _logger.LogInformation("\n\nSenderName: {0}\nSenderEmail: {1}\n", _settings.SenderName, _settings.SenderEmail);

                // 移除 App Password 中的空格
                // var cleanAppPassword = _settings.AppPassword.Replace(" ", "");

                // 2. 使用 MailKit 的 SmtpClient 傳送
                using (var client = new SmtpClient())
                {
                    _logger.LogInformation("\n\n{0}\n\n", client);
                    // 連接到 Gmail SMTP 伺服器
                    // MailKit 會自動處理 IPv4/IPv6，並使用 STARTTLS
                    await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);

                    _logger.LogInformation("\n成功連接到 SMTP 伺服器: {Server}:{Port}\n", _settings.SmtpServer, _settings.SmtpPort);

                    // 使用 App Password 進行身份驗證
                    await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);

                    _logger.LogInformation("\nSMTP 身份驗證成功，使用者: {User}\n", _settings.SenderName);

                    // 發送郵件
                    await client.SendAsync(message);

                    _logger.LogInformation("\n郵件已成功發送到: {Email}\n\n", toEmail);

                    // 斷開連接
                    await client.DisconnectAsync(true);
                }
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "\nMailKit SMTP 命令錯誤: StatusCode={StatusCode}, Message={Message}\n\n", ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\n使用 MailKit 發送郵件失敗到: {Email}. 錯誤類型: {Type}, 錯誤: {Message}\n\n", toEmail, ex.GetType().Name, ex.Message);
                throw;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string temporaryPassword)
        {
            try
            {
                string subject = "Matrix - 密碼重置";
                string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>Matrix</h1>
                        <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 16px;'>密碼重置通知</p>
                    </div>
                    
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                        <h2 style='color: #333; margin-top: 0;'>您好，{userName}</h2>
                        
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            我們已收到您的密碼重置請求。您的新臨時密碼是：
                        </p>
                        
                        <div style='background: #fff; border: 2px dashed #667eea; padding: 20px; margin: 20px 0; text-align: center; border-radius: 8px;'>
                            <h3 style='color: #667eea; font-family: monospace; font-size: 24px; margin: 0; letter-spacing: 2px;'>
                                {temporaryPassword}
                            </h3>
                        </div>
                        
                        <div style='background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h4 style='color: #856404; margin-top: 0;'>⚠️ 重要提醒</h4>
                            <ul style='color: #856404; margin: 10px 0; padding-left: 20px;'>
                                <li>請使用此臨時密碼登入系統</li>
                                <li>登入後系統會要求您立即修改密碼</li>
                                <li>為了您的帳戶安全，請盡快完成密碼修改</li>
                            </ul>
                        </div>
                        
                        <p style='color: #666; font-size: 14px; margin-top: 30px;'>
                            如果您沒有申請密碼重置，請忽略此郵件或聯繫我們的客服團隊。
                        </p>
                        
                        <p style='color: #666; font-size: 14px; margin-top: 20px;'>
                            祝您使用愉快！<br>
                            Matrix 團隊
                        </p>
                    </div>
                    
                    <div style='text-align: center; margin-top: 20px; color: #999; font-size: 12px;'>
                        <p>此郵件由系統自動發送，請勿直接回覆</p>
                        <p>© 2024 Matrix. All rights reserved.</p>
                    </div>
                </div>";

                await SendEmailAsync(toEmail, userName, subject, htmlBody);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                return false;
            }
        }

        /*
        // =================================================================================
        // 以下為使用已過時 System.Net.Mail.SmtpClient 的舊有實作
        // 微軟官方強烈不建議使用此類別，因其缺乏現代安全協議支援 (如 OAuth 2.0)
        // 且其異步方法並非真正的異步 I/O，可能導致效能問題。
        // 此處保留僅供參考。
        // =================================================================================

        public async Task SendEmailAsync_Old(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation("開始發送郵件到: {Email}", toEmail);
                
                // 移除 App Password 中的空格
                var cleanAppPassword = _settings.AppPassword.Replace(" ", "");
                
                _logger.LogDebug("SMTP 配置: Server={Server}, Port={Port}, EnableSsl={EnableSsl}", 
                    _settings.SmtpServer, _settings.SmtpPort, _settings.EnableSsl);
                
                // 參考可工作的 Gmail SMTP 範例設定
                MailMessage mms = new MailMessage();
                mms.From = new MailAddress(_settings.Id);
                mms.Subject = subject;
                mms.Body = htmlBody;
                mms.IsBodyHtml = true;
                mms.SubjectEncoding = Encoding.UTF8;
                mms.To.Add(new MailAddress(toEmail));

                using (SmtpClient client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_settings.Id, _settings.Secret);
                    
                    _logger.LogDebug("正在發送郵件...");
                    
                    // 使用同步方法，在 Task 中執行以保持 async 簽名
                    await Task.Run(() => client.Send(mms));
                    
                    _logger.LogInformation("郵件發送成功到: {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送郵件失敗到: {Email}. 錯誤: {Message}", toEmail, ex.Message);
                throw;
            }
        }
        */
    }
}
