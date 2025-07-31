namespace Matrix.DTOs
{
    public class GoogleSmtpDTOs
    {
        public string SenderEmail { get; set; } = null!;
        public string AppPassword { get; set; } = null!;
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SenderName { get; set; } = null!; // 新增寄件人名稱屬性
    }
}
