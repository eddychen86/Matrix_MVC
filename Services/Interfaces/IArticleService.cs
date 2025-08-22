using Matrix.DTOs;

namespace Matrix.Services.Interfaces
{
    /// <summary>
    /// 文章服務介面
    /// </summary>
    public interface IArticleService : ISearchableService<ArticleDto>, IStatusManageable<Guid>
    {
        /// <summary>
        /// 根據 ID 獲取文章資料
        /// </summary>
        Task<ArticleDto?> GetArticleAsync(Guid id);

        /// <summary>
        /// 建立新文章
        /// </summary>
        Task<bool> CreateArticleAsync(CreateArticleDto dto, Guid authorId);

        /// <summary>
        /// 更新文章內容
        /// </summary>
        Task<bool> UpdateArticleAsync(Guid id, string content, int isPublic, Guid authorId);

        /// <summary>
        /// 刪除文章
        /// </summary>
        Task<bool> DeleteArticleAsync(Guid id, Guid authorId);

        /// <summary>
        /// 分頁查詢文章列表
        /// </summary>
        Task<(List<ArticleDto> Articles, int TotalCount)> GetArticlesAsync(
            int page = 1,
            int pageSize = 20,
            string? searchKeyword = null,
            Guid? authorId = null,
            int? status = null,
            bool onlyPublic = true,
            DateTime? dateFrom = null,
            DateTime? dateTo = null
            );

        /// <summary>
        /// 增加文章讚數
        /// </summary>
        Task<bool> IncreasePraiseCountAsync(Guid articleId);

        /// <summary>
        /// 減少文章讚數
        /// </summary>
        Task<bool> DecreasePraiseCountAsync(Guid articleId);

        /// <summary>
        /// 增加文章收藏數
        /// </summary>
        Task<bool> IncreaseCollectCountAsync(Guid articleId);

        /// <summary>
        /// 減少文章收藏數
        /// </summary>
        Task<bool> DecreaseCollectCountAsync(Guid articleId);

        /// <summary>
        /// 新增文章回覆
        /// </summary>
        Task<bool> CreateReplyAsync(CreateReplyDto dto, Guid authorId);

        /// <summary>
        /// 獲取熱門文章
        /// </summary>
        Task<List<ArticleDto>> GetPopularArticlesAsync(int limit = 10, int days = 7);

        /// <summary>
        /// 建立一篇新文章，並處理其檔案附件
        /// </summary>
        Task<ArticleDto?> CreateArticleWithAttachmentsAsync(Guid authorId, CreateArticleDto dto);

        ///<summary>
        ///後臺管理員更新文章
        /// </summary>
        Task<bool> AdminUpdateArticleContentAsync(Guid id, string content);

        ///<summary>
        ///後臺管理員刪除文章
        /// </summary>
        Task<bool> AdminDeleteArticleAsync(Guid id);

        /// <summary>
        /// 獲取文章總數
        /// </summary>
        /// <param name="onlyPublic">是否只計算公開文章</param>
        /// <returns>文章總數</returns>
        Task<int> GetTotalArticlesCountAsync(bool onlyPublic = true);

        ///<summary>
        ///根據ID獲取文章全部內容
        /// </summary>
        Task<ArticleDto?> GetArticleDetailAsync(Guid articleId);

    }

}