using Matrix.Models;

namespace Matrix.Repository.Interfaces
{
    /// <summary>
    /// 回覆資料存取介面
    /// </summary>
    public interface IReplyRepository : IRepository<Reply>
    {
        /// <summary>根據文章ID取得回覆列表</summary>
        Task<IEnumerable<Reply>> GetByArticleIdAsync(Guid articleId);

        /// <summary>根據作者ID取得回覆列表</summary>
        Task<IEnumerable<Reply>> GetByAuthorIdAsync(Guid authorId);

        /// <summary>取得回覆的子回覆</summary>
        Task<IEnumerable<Reply>> GetChildRepliesAsync(Guid parentReplyId);

        /// <summary>取得回覆樹狀結構</summary>
        Task<IEnumerable<Reply>> GetReplyTreeAsync(Guid articleId);

        /// <summary>計算文章的回覆數量</summary>
        Task<int> CountRepliesByArticleAsync(Guid articleId);
    }
}