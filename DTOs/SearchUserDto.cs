namespace Matrix.DTOs
{
    public class SearchUserDto
    {
        public Guid PersonId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string AvatarPath { get; set; } = null!;
    }
}
