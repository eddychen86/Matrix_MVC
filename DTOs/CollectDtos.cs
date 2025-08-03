namespace Matrix.DTOs
{
    public class CollectItemDto
    {
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public DateTime CollectedAt { get; set; }
    }
}