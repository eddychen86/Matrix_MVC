using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    public interface IPraiseService
    {
        Task<ReturnType<bool>> TogglePraiseAsync(Guid articleId, Guid userId);
    }
}
