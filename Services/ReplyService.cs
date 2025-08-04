using Matrix.DTOs;
using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class ReplyService : IReplyService
    {
        private readonly IReplyRepository _replyRepository;
        private readonly IArticleRepository _articleRepository;

        public ReplyService(IReplyRepository replyRepository, IArticleRepository articleRepository)
        {
            _replyRepository = replyRepository;
            _articleRepository = articleRepository;
        }

        public async Task<ReturnType<ReplyDto>> CreateReplyAsync(Guid articleId, Guid userId, string content)
        {
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new ReturnType<ReplyDto> { Success = false, Message = "Article not found." };
            }

            var reply = new Reply
            {
                ArticleId = articleId,
                UserId = userId,
                Content = content,
                ReplyTime = DateTime.UtcNow
            };

            await _replyRepository.AddAsync(reply);

            var replyDto = new ReplyDto
            {
                ReplyId = reply.ReplyId,
                ArticleId = reply.ArticleId,
                AuthorId = reply.UserId,
                Content = reply.Content,
                CreateTime = reply.ReplyTime
            };

            return new ReturnType<ReplyDto> { Success = true, Data = replyDto };
        }

        public async Task<ReturnType<List<ReplyDto>>> GetRepliesByArticleIdAsync(Guid articleId)
        {
            var replies = await _replyRepository.GetByArticleIdAsync(articleId);
            var replyDtos = replies.Select(r => new ReplyDto
            {
                ReplyId = r.ReplyId,
                ArticleId = r.ArticleId,
                AuthorId = r.UserId,
                Content = r.Content,
                CreateTime = r.ReplyTime
            }).ToList();

            return new ReturnType<List<ReplyDto>> { Success = true, Data = replyDtos };
        }
    }
}
