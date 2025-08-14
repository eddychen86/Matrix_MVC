using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    public interface IReplyService
    {
        Task<ReturnType<ReplyDto>> CreateReplyAsync(Guid articleId, Guid userId, string content);
        Task<ReturnType<List<ReplyDto>>> GetRepliesByArticleIdAsync(Guid articleId);
    }
}
