using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 回覆資料存取實作
    /// </summary>
    public class ReplyRepository(ApplicationDbContext context) : BaseRepository<Reply>(context), IReplyRepository
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

        public Task<IEnumerable<Reply>> GetChildRepliesAsync(Guid parentReplyId)
        {
            // 注意：目前的 Reply Model 不支援巢狀回覆 (缺少 ParentReplyId)，因此此功能無法實作。
            // return Task.FromResult(Enumerable.Empty<Reply>());
            throw new NotImplementedException("Reply Model does not support child replies.");
        }

        public Task<IEnumerable<Reply>> GetReplyTreeAsync(Guid articleId)
        {
            // 注意：目前的 Reply Model 不支援巢狀回覆，因此無法建構回覆樹。
            // 僅回傳該文章的第一層回覆。
            return GetByArticleIdAsync(articleId);
        }

        public async Task<int> CountRepliesByArticleAsync(Guid articleId)
        {
            return await _dbSet.CountAsync(r => r.ArticleId == articleId);
        }
    }
}
