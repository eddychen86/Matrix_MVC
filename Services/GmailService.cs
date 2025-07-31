using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using Matrix.DTOs;

namespace Matrix.Services
{
    public class GmailService : IEmailService
    {
        private readonly GoogleSmtpDTOs _settings;

        public GmailService(IOptions<GoogleSmtpDTOs> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            // 使用 Google SMTP 服務發送郵件
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_settings.SenderEmail);
                message.To.Add(new MailAddress(toEmail, toName));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;
                message.SubjectEncoding = Encoding.UTF8;
                message.BodyEncoding = Encoding.UTF8;

                using (var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort))
                {
                    client.EnableSsl = _settings.EnableSsl;
                    client.Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword);
                    
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}
