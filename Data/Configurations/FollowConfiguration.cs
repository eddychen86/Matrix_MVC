using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 追蹤實體的配置類別
    /// </summary>
    public class FollowConfiguration : IEntityTypeConfiguration<Follow>
    {
        public void Configure(EntityTypeBuilder<Follow> builder)
        {
            /// <summary>
            /// 設定追蹤者關聯，一人可追蹤多個對象，使用 UserId 作為外鍵，限制刪除以維護追蹤關係
            /// FollowedId 不設外鍵，可指向不同實體類型
            /// </summary>
            builder.HasOne(f => f.User)
                .WithMany(p => p.Follows)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}