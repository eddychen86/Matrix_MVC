using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    public interface ICollectService
    {
        Task<IEnumerable<CollectItemDto>> GetUserCollectsAsync(Guid userId, int take = 30);
        Task<ReturnType<bool>> ToggleCollectAsync(Guid articleId, Guid userId);
    }
}