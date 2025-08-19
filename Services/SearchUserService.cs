using Microsoft.IdentityModel.Tokens;

namespace Matrix.Services
{
    public class SearchUserService : ISearchUserService
    {
        private readonly IPersonRepository _personRepository;

        public SearchUserService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<IEnumerable<SearchUserDto>> SearchUsersAsync(string keyword)
        {
             var users = await _personRepository.SearchByDisplayNameAsync(keyword);

            return users.Select(p => new SearchUserDto
            {
                PersonId = p.PersonId,
                DisplayName = p.DisplayName ?? "無名氏",
                AvatarPath = p.AvatarPath ?? "/static/img/cute.png"

            });
        }
    }
}
