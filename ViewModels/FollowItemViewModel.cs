namespace Matrix.ViewModels
{
    public class FollowItemViewModel
    {
        public Guid PersonId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatarUrl { get; set; }
        public DateTime FollowTime { get; set; }
    }
}
