using Matrix.DTOs;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class PraiseService : IPraiseService
    {
        private readonly IPraiseCollectRepository _praiseCollectRepository;
        private readonly IArticleRepository _articleRepository;

        public PraiseService(IPraiseCollectRepository praiseCollectRepository, IArticleRepository articleRepository)
        {
            _praiseCollectRepository = praiseCollectRepository;
            _articleRepository = articleRepository;
        }

        public async Task<ReturnType<bool>> TogglePraiseAsync(Guid articleId, Guid userId)
        {
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new ReturnType<bool> { Success = false, Message = "Article not found." };
            }

            var hasPraised = await _praiseCollectRepository.HasUserPraisedAsync(userId, articleId);
            if (hasPraised)
            {
                await _praiseCollectRepository.UpdatePraiseStatusAsync(userId, articleId, false);
                await _articleRepository.DecreasePraiseCountAsync(articleId);
            }
            else
            {
                await _praiseCollectRepository.UpdatePraiseStatusAsync(userId, articleId, true);
                await _articleRepository.IncreasePraiseCountAsync(articleId);
            }

            return new ReturnType<bool> { Success = true, Data = !hasPraised };
        }
    }
}
