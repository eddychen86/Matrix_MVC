namespace Matrix.Services.Interfaces
{
    public interface ISearchUserService
    {
        Task<IEnumerable<SearchUserDto>> SearchUsersAsync(string keyword);
    }
}
