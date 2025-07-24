namespace Matrix.ViewModels
{
    public class CollectItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public DateTime CollectedAt { get; set; }
    }
}
