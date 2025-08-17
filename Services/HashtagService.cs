using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    /// <summary>
    /// 標籤服務實作
    /// </summary>
    public class HashtagService : IHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;

        public HashtagService(IHashtagRepository hashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
        }

        public async Task<Hashtag?> GetHashtagByNameAsync(string name)
        {
            return await _hashtagRepository.GetByNameAsync(name);
        }

        public async Task<List<Hashtag>> GetPopularHashtagsAsync(int count = 10)
        {
            var hashtags = await _hashtagRepository.GetPopularTagsAsync(count);
            return hashtags.ToList();
        }

        public async Task<(List<Hashtag> Hashtags, int TotalCount)> SearchHashtagsAsync(string keyword, int page = 1, int pageSize = 20)
        {
            var hashtags = await _hashtagRepository.SearchTagsAsync(keyword, page, pageSize);
            var hashtagList = hashtags.ToList();
            return (hashtagList, hashtagList.Count);
        }

        public async Task<int> GetHashtagUsageCountAsync(Guid hashtagId)
        {
            return await _hashtagRepository.GetTagUsageCountAsync(hashtagId);
        }

        public async Task<List<(Hashtag Tag, int UsageCount)>> GetAllHashtagsWithUsageCountAsync()
        {
            var result = await _hashtagRepository.GetAllTagsWithUsageCountAsync();
            return result.ToList();
        }

        public async Task<List<Hashtag>> GetOrCreateHashtagsAsync(List<string> tagNames)
        {
            var hashtags = await _hashtagRepository.GetOrCreateTagsAsync(tagNames);
            return hashtags.ToList();
        }

        public async Task<bool> CleanupUnusedHashtagsAsync()
        {
            try
            {
                await _hashtagRepository.CleanupUnusedTagsAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}