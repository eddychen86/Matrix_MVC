using Microsoft.EntityFrameworkCore;
using Matrix.DTOs;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class CollectService : ICollectService
    {
        private readonly IPraiseCollectRepository _praiseCollectRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<CollectService> _logger;

        public CollectService(
            IPraiseCollectRepository praiseCollectRepository,
            IArticleRepository articleRepository, // Add this
            ILogger<CollectService> logger)
        {
            _praiseCollectRepository = praiseCollectRepository;
            _articleRepository = articleRepository;
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
                    ImageUrl = p.Article?.Attachments?.Where(a => a.Type == "image").Select(a => a.FilePath).FirstOrDefault() ?? "/static/img/cute.png",
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

        public async Task<ReturnType<bool>> ToggleCollectAsync(Guid articleId, Guid userId)
        {
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new ReturnType<bool> { Success = false, Message = "Article not found." };
            }

            var hasCollected = await _praiseCollectRepository.HasUserCollectedAsync(userId, articleId);
            if (hasCollected)
            {
                await _praiseCollectRepository.UpdateCollectStatusAsync(userId, articleId, false);
                await _articleRepository.DecreaseCollectCountAsync(articleId);
            }
            else
            {
                await _praiseCollectRepository.UpdateCollectStatusAsync(userId, articleId, true);
                await _articleRepository.IncreaseCollectCountAsync(articleId);
            }

            return new ReturnType<bool> { Success = true, Data = !hasCollected };
        }
    }
}