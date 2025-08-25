using System;

namespace Matrix.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string temporaryPassword);
    }
}
