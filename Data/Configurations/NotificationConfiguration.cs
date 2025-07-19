using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Matrix.Models;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// Notification 實體的 Entity Framework Core 配置類別
    ///
    /// 此檔案用於配置 Notification 實體的資料庫映射設定，包括：
    /// - 主鍵配置
    /// - 欄位長度限制和驗證規則
    /// - 索引設定
    /// - 預設值設定
    /// - 欄位是否必填的約束
    ///
    /// 注意事項：
    /// - 僅能新增與 Notification 實體相關的資料庫配置
    /// - 不應在此檔案中配置與其他實體的關聯性（應在 ApplicationDbContext.OnModelCreating 中處理）
    /// - 所有配置都必須符合 Notification 模型的定義
    /// - 修改此檔案後需要建立新的 Migration
    /// </summary>
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        /// <summary>
        /// 配置 Notification 實體的資料庫映射規則
        /// </summary>
        /// <param name="builder">實體類型建構器</param>
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // === 主鍵配置 ===
            /// <summary>
            /// 設定 NotifyId 作為主鍵
            /// 用途：定義 Notification 實體的唯一識別碼
            /// </summary>
            builder.HasKey(n => n.NotifyId);

            builder.Property(n => n.NotifyId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // === 欄位配置 ===
            /// <summary>
            /// 配置 GetId 欄位屬性
            /// 用途：設定接收通知的用戶識別碼
            /// 必填：是
            /// </summary>
            builder.Property(n => n.GetId)
                .IsRequired();

            /// <summary>
            /// 配置 SendId 欄位屬性
            /// 用途：設定發送通知的用戶識別碼
            /// 必填：是
            /// </summary>
            builder.Property(n => n.SendId)
                .IsRequired();

            /// <summary>
            /// 配置 Type 欄位屬性
            /// 用途：設定通知類型的資料庫約束
            /// 必填：是
            /// </summary>
            builder.Property(n => n.Type)
                .IsRequired();

            /// <summary>
            /// 配置 IsRead 欄位屬性
            /// 用途：設定通知閱讀狀態的資料庫約束
            /// 預設值：0 （未讀）
            /// 必填：是
            /// </summary>
            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(0);

            /// <summary>
            /// 配置 SentTime 欄位屬性
            /// 用途：設定通知發送時間的資料庫約束
            /// 必填：是
            /// 預設值：當前時間
            /// </summary>
            builder.Property(n => n.SentTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            /// <summary>
            /// 配置 IsReadTime 欄位屬性
            /// 用途：設定通知閱讀時間的資料庫約束
            /// 必填：否
            /// </summary>
            builder.Property(n => n.IsReadTime)
                .IsRequired(false);

            // === 索引配置 ===
            /// <summary>
            /// 為 GetId 建立索引
            /// 用途：提高按接收者查詢通知的效能
            /// </summary>
            builder.HasIndex(n => n.GetId)
                .HasDatabaseName("IX_Notifications_GetId");

            /// <summary>
            /// 為 SendId 建立索引
            /// 用途：提高按發送者查詢通知的效能
            /// </summary>
            builder.HasIndex(n => n.SendId)
                .HasDatabaseName("IX_Notifications_SendId");

            /// <summary>
            /// 為 Type 建立索引
            /// 用途：提高按通知類型查詢的效能
            /// </summary>
            builder.HasIndex(n => n.Type)
                .HasDatabaseName("IX_Notifications_Type");

            /// <summary>
            /// 為 IsRead 建立索引
            /// 用途：提高按閱讀狀態查詢通知的效能
            /// </summary>
            builder.HasIndex(n => n.IsRead)
                .HasDatabaseName("IX_Notifications_IsRead");

            /// <summary>
            /// 為 SentTime 建立索引
            /// 用途：提高按發送時間查詢和排序通知的效能
            /// </summary>
            builder.HasIndex(n => n.SentTime)
                .HasDatabaseName("IX_Notifications_SentTime");

            /// <summary>
            /// 建立複合索引：GetId + IsRead + SentTime
            /// 用途：提高查詢特定用戶的未讀通知並按時間排序的效能
            /// </summary>
            builder.HasIndex(n => new { n.GetId, n.IsRead, n.SentTime })
                .HasDatabaseName("IX_Notifications_GetId_IsRead_SentTime");

            /// <summary>
            /// 建立複合索引：GetId + Type + SentTime
            /// 用途：提高查詢特定用戶特定類型的通知並按時間排序的效能
            /// </summary>
            builder.HasIndex(n => new { n.GetId, n.Type, n.SentTime })
                .HasDatabaseName("IX_Notifications_GetId_Type_SentTime");

            /// <summary>
            /// 建立複合索引：SendId + SentTime
            /// 用途：提高查詢特定發送者的通知並按時間排序的效能
            /// </summary>
            builder.HasIndex(n => new { n.SendId, n.SentTime })
                .HasDatabaseName("IX_Notifications_SendId_SentTime");

            // === 表格名稱配置 ===
            /// <summary>
            /// 設定資料庫表格名稱
            /// 用途：明確指定資料庫中的表格名稱
            /// </summary>
            builder.ToTable("Notifications");
        }
    }
}
