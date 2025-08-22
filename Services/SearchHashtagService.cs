namespace Matrix.Services
{
    public class SearchHashtagService : ISearchHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;
        private readonly ApplicationDbContext _db;   // ← 新增：用來查文章

        public SearchHashtagService(IHashtagRepository hashtagRepository, ApplicationDbContext db)
        {
            _hashtagRepository = hashtagRepository;
            _db = db;
        }

        public async Task<IEnumerable<SearchHashtagDto>> SearchHashtagsAsync(string keyword)
        {
            // 呼叫組長寫好的方法
            var hashtags = await _hashtagRepository.SearchTagsAsync(keyword, page: 1, pageSize: 10);

            return hashtags.Select(h => new SearchHashtagDto
            {
                TagId = h.TagId,
                Content = h.Content ?? ""
            });
        }
        // ② 新增：用 hashtag 撈文章（分頁）
        public async Task<(int TotalCount, List<TagArticleItemDto> Items)> GetArticlesByTagAsync(string tag, int page, int pageSize)
        {
            tag = (tag ?? "").Trim();
            if (string.IsNullOrEmpty(tag)) return (0, new());
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 50) pageSize = 10;

            // Articles 依你的 Article 模型（Author/Attachments/ArticleHashtags）
            var q = _db.Articles
                .AsNoTracking()
                .Include(a => a.Attachments)
                .Include(a => a.Author)                  // Person 導航屬性
                .Where(a => a.Status == 0 && a.IsPublic == 0)
                .Where(a => a.ArticleHashtags!
                .Any(ah => ah.Hashtag != null && ah.Hashtag.Content == tag));
            // 如需大小寫不敏感且資料庫 Collation 不是 CI，可改：
            // .Any(ah => ah.Hashtag != null && EF.Functions.Like(ah.Hashtag.Content, tag))

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new TagArticleItemDto
                {
                    ArticleId = a.ArticleId,
                    Content = a.Content,
                    CreateTime = a.CreateTime,
                    AuthorName = (a.Author != null ? a.Author.DisplayName : null) ?? "",
                    AuthorAvatar = a.Author != null ? a.Author.AvatarPath : "",
                    Images = a.Attachments != null ? a.Attachments.Select(x => x.FilePath).ToList() : new List<string>()
                })
                .ToListAsync();

            return (total, items);
        }
    }
}
