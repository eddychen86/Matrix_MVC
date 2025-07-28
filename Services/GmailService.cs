using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace Matrix.Services
{
    public class GmailService : IEmailService
    {
        private readonly GoogleOAuthDTOs _settings;

        public GmailService(IOptions<GoogleOAuthDTOs> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            // --- 步驟 1: 使用 Refresh Token 取得新的 Access Token ---
            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _settings.ClientId,
                            ClientSecret = _settings.ClientSecret
                        }
                    }),
                "user", // "user" ID can be any string
                new TokenResponse { RefreshToken = _settings.RefreshToken });

            // 強制刷新以確保我们有一個有效的 Access Token
            // Google 函式庫會處理快取，所以不會每次都真的去請求，除非 Token 快過期了
            await credential.RefreshTokenAsync(CancellationToken.None);
            var accessToken = credential.Token.AccessToken;


            // --- 步驟 2: 使用 MailKit 建立並發送郵件 ---
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("您的網站名稱", _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlBody;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // 使用 OAuth 2.0 進行驗證
                var oauth2 = new SaslMechanismOAuth2(_settings.SenderEmail, accessToken);
                await client.AuthenticateAsync(oauth2);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
