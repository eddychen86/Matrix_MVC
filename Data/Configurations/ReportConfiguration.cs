using Matrix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 檢舉實體的配置類別
    /// </summary>
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            /// <summary>
            /// 設定舉報者關聯，一人可提出多個檢舉，使用 ReporterId 作為外鍵，限制刪除以保留舉報歷史
            /// </summary>
            builder.HasOne(r => r.Reporter)
                .WithMany(p => p.ReportsMade)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定檢舉處理者關聯，一人可處理多個檢舉，使用 ResolverId 作為外鍵，限制刪除以保留處理歷史
            /// </summary>
            builder.HasOne(r => r.Resolver)
                .WithMany(p => p.ReportsResolved)
                .HasForeignKey(r => r.ResolverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}