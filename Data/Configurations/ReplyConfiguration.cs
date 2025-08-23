using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 回覆實體的配置類別
    /// </summary>
    public class ReplyConfiguration : IEntityTypeConfiguration<Reply>
    {
        public void Configure(EntityTypeBuilder<Reply> builder)
        {
            /// <summary>
            /// 設定使用者回覆關聯，一人可發表多個回覆，使用 UserId 作為外鍵，限制刪除以保留回覆歷史
            /// </summary>
            builder.HasOne(r => r.User)
                .WithMany(p => p.Replies)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定文章回覆關聯，一篇文章可有多個回覆，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            builder.HasOne(r => r.Article)
                .WithMany(a => a.Replies)
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}