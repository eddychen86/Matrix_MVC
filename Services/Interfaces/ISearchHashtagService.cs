namespace Matrix.Services.Interfaces
{
    public interface ISearchHashtagService
    {
        Task<IEnumerable<SearchHashtagDto>> SearchHashtagsAsync(string keyword);
    }
}
