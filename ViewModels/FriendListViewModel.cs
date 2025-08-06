namespace Matrix.ViewModels
{
    public class FriendListViewModel
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? AvatarPath {  get; set; }
    }
}
