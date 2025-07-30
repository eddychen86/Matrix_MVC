using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Matrix.Repository.Interfaces;

namespace Matrix.Repository
{
    /// <summary>
    /// 回覆資料存取實作
    /// </summary>
    public class ReplyRepository : BaseRepository<Reply>, IReplyRepository
    {
        public ReplyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reply>> GetByArticleIdAsync(Guid articleId)
        {
            return await _dbSet
                .Include(r => r.Author)
                .Where(r => r.ArticleId == articleId)
                .OrderBy(r => r.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reply>> GetByAuthorIdAsync(Guid authorId)
        {
            return await _dbSet
                .Include(r => r.Article)
                .Where(r => r.AuthorId == authorId)
                .OrderByDescending(r => r.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reply>> GetChildRepliesAsync(Guid parentReplyId)
        {
            return await _dbSet
                .Include(r => r.Author)
                .Where(r => r.ParentReplyId == parentReplyId)
                .OrderBy(r => r.CreateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reply>> GetReplyTreeAsync(Guid articleId)
        {
            // 取得所有該文章的回覆（包含子回覆）
            return await _dbSet
                .Include(r => r.Author)
                .Where(r => r.ArticleId == articleId)
                .OrderBy(r => r.ParentReplyId == null ? 0 : 1) // 父回覆優先
                .ThenBy(r => r.CreateTime)
                .ToListAsync();
        }

        public async Task<int> CountRepliesByArticleAsync(Guid articleId)
        {
            return await _dbSet.CountAsync(r => r.ArticleId == articleId);
        }
    }
}