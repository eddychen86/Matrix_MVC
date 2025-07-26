using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;
using Matrix.Models;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html) { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
