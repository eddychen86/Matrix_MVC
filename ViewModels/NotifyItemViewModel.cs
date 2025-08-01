namespace Matrix.ViewModels
{
    public class NotifyItemViewModel
    {
        public string SenderName { get; set; } = null!;
        public string SenderAvatarUrl { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SentTime { get; set; }
    }
}
