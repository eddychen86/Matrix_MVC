using System;

namespace Matrix.DTOs
{
    public class GoogleOAuthDTOs
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public string SenderEmail { get; set; } = "";
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
