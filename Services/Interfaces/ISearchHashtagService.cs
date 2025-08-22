namespace Matrix.Services.Interfaces
{
    public interface ISearchHashtagService
    {
        Task<IEnumerable<SearchHashtagDto>> SearchHashtagsAsync(string keyword);

        Task<(int TotalCount, List<TagArticleItemDto> Items)>
        GetArticlesByTagAsync(string tag, int page, int pageSize);
    }
}
