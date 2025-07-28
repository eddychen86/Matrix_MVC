using System;

namespace Matrix.DTOs
{
    public class GoogleOAuthDTOs
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string RefreshToken { get; set; }
        public required string SenderEmail { get; set; }
    }

    public class GoogleTokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required string TokenType { get; set; }
        public required int ExpiresIn { get; set; }
    }

    public class GoogleUserInfo
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Picture { get; set; }
    }
}
