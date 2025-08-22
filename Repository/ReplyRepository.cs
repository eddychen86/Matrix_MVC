using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    public class ReplyRepository(ApplicationDbContext context)
        : BaseRepository<Reply>(context), IReplyRepository
    {
        public async Task<IEnumerable<Reply>> GetByArticleIdAsync(Guid articleId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Where(r => r.ArticleId == articleId)
                .OrderBy(r => r.ReplyTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reply>> GetByAuthorIdAsync(Guid authorId)
        {
            return await _dbSet
                .Include(r => r.Article)
                .Where(r => r.UserId == authorId)
                .OrderByDescending(r => r.ReplyTime)
                .ToListAsync();
        }

        public async Task<Reply?> GetByIdWithUserAsync(Guid replyId)
        {
            return await _dbSet
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReplyId == replyId);
        }

        public Task<IEnumerable<Reply>> GetChildRepliesAsync(Guid parentReplyId)
        {
            throw new NotImplementedException("Reply Model does not support child replies.");
        }

        public Task<IEnumerable<Reply>> GetReplyTreeAsync(Guid articleId)
        {
            return GetByArticleIdAsync(articleId);
        }

        public async Task<int> CountRepliesByArticleAsync(Guid articleId)
        {
            return await _dbSet.CountAsync(r => r.ArticleId == articleId);
        }

        public async Task<IEnumerable<Reply>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReplyTime)
                .ToListAsync();
        }
    }
}
