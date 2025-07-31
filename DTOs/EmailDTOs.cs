namespace Matrix.DTOs
{
    public class GoogleSmtpDTOs
    {
        public string SenderEmail { get; set; } = "";
        public string AppPassword { get; set; } = "";
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
    }
}
