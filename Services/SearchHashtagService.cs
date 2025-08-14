namespace Matrix.Services
{
    public class SearchHashtagService : ISearchHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;

        public SearchHashtagService(IHashtagRepository hashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
        }

        public async Task<IEnumerable<SearchHashtagDto>> SearchHashtagsAsync(string keyword)
        {
            // 呼叫組長寫好的方法
            var hashtags = await _hashtagRepository.SearchTagsAsync(keyword, page: 1, pageSize: 10);

            return hashtags.Select(h => new SearchHashtagDto
            {
                TagId = h.TagId,
                Content = h.Content ?? ""
            });
        }
    }
}
