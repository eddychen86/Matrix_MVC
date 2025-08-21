using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 讚美收藏實體的配置類別
    /// </summary>
    public class PraiseCollectConfiguration : IEntityTypeConfiguration<PraiseCollect>
    {
        public void Configure(EntityTypeBuilder<PraiseCollect> builder)
        {
            /// <summary>
            /// 設定使用者讚美收藏關聯，一人可有多個讚美收藏，使用 UserId 作為外鍵，限制刪除以保留用戶行為
            /// </summary>
            builder.HasOne(pc => pc.User)
                .WithMany(p => p.PraiseCollects)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定文章讚美收藏關聯，一篇文章可有多個讚美收藏，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            builder.HasOne(pc => pc.Article)
                .WithMany(a => a.PraiseCollects)
                .HasForeignKey(pc => pc.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}