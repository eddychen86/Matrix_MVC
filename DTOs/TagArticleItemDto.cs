using System.Text.Json.Serialization;

namespace Matrix.DTOs
{
    /// <summary>
    /// 以標籤查詢時回傳的文章清單項目
    /// </summary>
    public sealed class TagArticleItemDto
    {
        public Guid ArticleId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreateTime { get; set; }
        public string AuthorName { get; set; } = "";
        public string? AuthorAvatar { get; set; }
        public List<string> Images { get; set; } = new();
    }
}
