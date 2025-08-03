using Microsoft.EntityFrameworkCore;
using Matrix.DTOs;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class CollectService : ICollectService
    {
        private readonly IPraiseCollectRepository _praiseCollectRepository;
        private readonly ILogger<CollectService> _logger;

        public CollectService(
            IPraiseCollectRepository praiseCollectRepository,
            ILogger<CollectService> logger)
        {
            _praiseCollectRepository = praiseCollectRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CollectItemDto>> GetUserCollectsAsync(Guid userId, int take = 30)
        {
            try
            {
                var collects = await _praiseCollectRepository.GetUserCollectionsAsync(userId, 1, take);
                
                return collects.Select(p => new CollectItemDto
                {
                    Title = p.Article?.Content?.Substring(0, Math.Min(10, p.Article.Content?.Length ?? 0)) ?? "",
                    ImageUrl = p.Article?.Attachments?.Where(a => a.Type == "image").Select(a => a.FilePath).FirstOrDefault() ?? "/static/img/Cute.png",
                    AuthorName = p.User?.DisplayName ?? "匿名",
                    CollectedAt = p.CreateTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user collections for userId: {UserId}", userId);
                return Enumerable.Empty<CollectItemDto>();
            }
        }
    }
}