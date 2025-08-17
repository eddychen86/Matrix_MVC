using Matrix.DTOs;
using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using AutoMapper;

namespace Matrix.Services
{
    public class ReplyService : IReplyService
    {
        private readonly IReplyRepository _replyRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly IMapper _mapper;

        public ReplyService(IReplyRepository replyRepository, IArticleRepository articleRepository, IMapper mapper)
        {
            _replyRepository = replyRepository;
            _articleRepository = articleRepository;
            _mapper = mapper;
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

            var replyDto = _mapper.Map<ReplyDto>(reply);
            return new ReturnType<ReplyDto> { Success = true, Data = replyDto };
        }

        public async Task<ReturnType<List<ReplyDto>>> GetRepliesByArticleIdAsync(Guid articleId)
        {
            var replies = await _replyRepository.GetByArticleIdAsync(articleId);
            var replyDtos = _mapper.Map<List<ReplyDto>>(replies);
            return new ReturnType<List<ReplyDto>> { Success = true, Data = replyDtos };
        }
    }
}
