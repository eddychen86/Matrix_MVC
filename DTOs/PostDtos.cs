namespace Matrix.DTOs
{
    public class GetAllPostsRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10; // 預設 10 篇
        public Guid? AuthorId { get; set; }
    }

    public class GetMyPostsRequestDto
    {
        public int Page { get; set; } = 1;
    }
}
