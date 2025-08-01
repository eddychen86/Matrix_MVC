namespace Matrix.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 0;
        public string SmtpUser { get; set; } = null!;
        public string SmtpPass { get; set; } = null!;
        public string FromAddress { get; set; } = null!;
        public string FromName { get; set; } = null!;
    }
}
