using AutoMapper;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Repository;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;

namespace Matrix.Services
{
    public class ReplyService : IReplyService
    {
        private readonly IReplyRepository _replyRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public ReplyService(IReplyRepository replyRepository, IArticleRepository articleRepository, IMapper mapper, IPersonRepository personRepository)
        {
            _replyRepository = replyRepository;
            _articleRepository = articleRepository;
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<ReturnType<ReplyDto>> CreateReplyAsync(Guid articleId, Guid userId, string content)
        {
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new ReturnType<ReplyDto> { Success = false, Message = "Article not found." };
            }

            // 這裡把 UserId 轉成 PersonId
            var person = await _personRepository.GetByUserIdAsync(userId);
            if (person == null)
            {
                return new ReturnType<ReplyDto> { Success = false, Message = "Person profile not found for current user." };
            }

            var reply = new Reply
            {
                ArticleId = articleId,
                UserId = person.PersonId,
                Content = content,
                ReplyTime = DateTime.UtcNow
            };

            await _replyRepository.AddAsync(reply);

            // 重新載入（含作者資訊）
            var saved = await _replyRepository.GetByIdWithUserAsync(reply.ReplyId);
            if (saved == null)
            {
                return new ReturnType<ReplyDto> { Success = false, Message = "Reply not found after creation." };
            }

            var replyDto = _mapper.Map<ReplyDto>(saved);
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
