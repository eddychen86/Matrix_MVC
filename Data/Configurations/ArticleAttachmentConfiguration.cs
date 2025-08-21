using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 文章附件實體的配置類別
    /// </summary>
    public class ArticleAttachmentConfiguration : IEntityTypeConfiguration<ArticleAttachment>
    {
        public void Configure(EntityTypeBuilder<ArticleAttachment> builder)
        {
            /// <summary>
            /// 設定文章附件關聯，一篇文章可有多個附件，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            builder.HasOne(aa => aa.Article)
                .WithMany(a => a.Attachments)
                .HasForeignKey(aa => aa.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}