namespace Matrix.DTOs
{
    public class FollowUserSearchDto
    {
        public Guid PersonId { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarPath { get; set; }
        public bool IsFollowed { get; set; }
    }
}
