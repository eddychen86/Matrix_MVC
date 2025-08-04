namespace Matrix.DTOs
{
    public class GetAllPostsRequestDto
    {
        public int Page { get; set; } = 1;
        public Guid? AuthorId { get; set; }
    }
}
