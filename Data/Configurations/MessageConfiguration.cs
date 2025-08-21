using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// Message 實體的 Entity Framework Core 配置類別
    /// </summary>
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        /// <summary>
        /// 配置 Message 實體的資料庫映射規則
        /// </summary>
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            /// <summary>
            /// 設定 Message 與 Person 的關聯，使用 SentId 作為外鍵連接到發送者
            /// </summary>
            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.SentId)
                .OnDelete(DeleteBehavior.Restrict);

            // === 主鍵配置 ===
            builder.HasKey(m => m.MsgId);

            builder.Property(m => m.MsgId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // === 欄位配置 ===
            builder.Property(m => m.SentId)
                .IsRequired();

            builder.Property(m => m.ReceiverId)
                .IsRequired();

            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(m => m.CreateTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(m => m.IsRead)
                .IsRequired()
                .HasDefaultValue(0);
        }
    }
}