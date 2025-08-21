using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 文章標籤關聯實體的配置類別
    /// </summary>
    public class ArticleHashtagConfiguration : IEntityTypeConfiguration<ArticleHashtag>
    {
        public void Configure(EntityTypeBuilder<ArticleHashtag> builder)
        {
            /// <summary>
            /// 設定 ArticleHashtag 的複合主鍵，確保文章和標籤間多對多關聯的唯一性
            /// </summary>
            builder.HasKey(ah => new { ah.ArticleId, ah.TagId });
        }
    }
}