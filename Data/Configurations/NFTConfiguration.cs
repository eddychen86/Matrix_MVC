using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    public class NFTConfiguration : IEntityTypeConfiguration<NFT>
    {
        public void Configure(EntityTypeBuilder<NFT> builder)
        {
            /// <summary>
            /// 主鍵配置
            /// </summary>
            builder.HasKey(n => n.NftId);

            /// <summary>
            /// 關聯關係：一個 Person 擁有多個 NFT
            /// </summary>
            builder.HasOne(n => n.Owner)
                    .WithMany(p => p.NFTs)
                    .HasForeignKey(n => n.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);

            /// <summary>
            /// 欄位長度限制
            /// </summary>
            builder.Property(n => n.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

            builder.Property(n => n.FilePath)
                    .IsRequired()
                    .HasMaxLength(2048);

            builder.Property(n => n.Currency)
                    .IsRequired()
                    .HasMaxLength(10);

            /// <summary>
            /// 價格欄位精確度：28位數總長度，18位小數
            /// </summary>
            builder.Property(n => n.Price)
                    .HasPrecision(28, 18);

            /// <summary>
            /// 索引優化：提升查詢效能
            /// </summary>
            builder.HasIndex(n => n.OwnerId)
                    .HasDatabaseName("IX_NFTs_OwnerId");

            builder.HasIndex(n => n.CollectTime)
                    .HasDatabaseName("IX_NFTs_CollectTime");

            builder.HasIndex(n => n.Currency)
                    .HasDatabaseName("IX_NFTs_Currency");

            /// <summary>
            /// 資料表名稱設定
            /// </summary>
            builder.ToTable("NFTs");
        }
    }
}