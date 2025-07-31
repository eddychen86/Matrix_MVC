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
