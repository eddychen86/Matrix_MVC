using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Matrix.Models;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// Friendship 實體的 Entity Framework Core 配置類別
    ///
    /// 此檔案用於配置 Friendship 實體的資料庫映射設定，包括：
    /// - 主鍵配置
    /// - 欄位長度限制和驗證規則
    /// - 索引設定
    /// - 預設值設定
    /// - 欄位是否必填的約束
    /// - 列舉類型的轉換設定
    ///
    /// 注意事項：
    /// - 僅能新增與 Friendship 實體相關的資料庫配置
    /// - 不應在此檔案中配置與其他實體的關聯性（應在 ApplicationDbContext.OnModelCreating 中處理）
    /// - 所有配置都必須符合 Friendship 模型的定義
    /// - 修改此檔案後需要建立新的 Migration
    /// </summary>
    public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
    {
        /// <summary>
        /// 配置 Friendship 實體的資料庫映射規則
        /// </summary>
        /// <param name="builder">實體類型建構器</param>
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            // === 主鍵配置 ===
            /// <summary>
            /// 設定 FriendshipId 作為主鍵
            /// 用途：定義 Friendship 實體的唯一識別碼
            /// </summary>
            builder.HasKey(f => f.FriendshipId);

            builder.Property(f => f.FriendshipId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // === 欄位配置 ===
            /// <summary>
            /// 配置 UserId 欄位屬性
            /// 用途：設定發出好友邀請的用戶識別碼
            /// 必填：是
            /// </summary>
            builder.Property(f => f.UserId)
                .IsRequired();

            /// <summary>
            /// 配置 FriendId 欄位屬性
            /// 用途：設定接收好友邀請的用戶識別碼
            /// 必填：是
            /// </summary>
            builder.Property(f => f.FriendId)
                .IsRequired();

            /// <summary>
            /// 配置 Status 欄位屬性
            /// 用途：設定好友關係狀態的資料庫約束
            /// 必填：是
            /// 預設值：Pending（待確認）
            /// 轉換：將列舉轉換為字串儲存
            /// </summary>
            builder.Property(f => f.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(FriendshipStatus.Pending);

            /// <summary>
            /// 配置 RequestDate 欄位屬性
            /// 用途：設定好友邀請發送時間的資料庫約束
            /// 必填：是
            /// 預設值：當前時間
            /// </summary>
            builder.Property(f => f.RequestDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // === 索引配置 ===
            /// <summary>
            /// 為 UserId 建立索引
            /// 用途：提高按發送者查詢好友關係的效能
            /// </summary>
            builder.HasIndex(f => f.UserId)
                .HasDatabaseName("IX_Friendships_UserId");

            /// <summary>
            /// 為 FriendId 建立索引
            /// 用途：提高按接收者查詢好友關係的效能
            /// </summary>
            builder.HasIndex(f => f.FriendId)
                .HasDatabaseName("IX_Friendships_FriendId");

            /// <summary>
            /// 為 Status 建立索引
            /// 用途：提高按狀態查詢好友關係的效能
            /// </summary>
            builder.HasIndex(f => f.Status)
                .HasDatabaseName("IX_Friendships_Status");

            /// <summary>
            /// 為 RequestDate 建立索引
            /// 用途：提高按邀請時間查詢和排序好友關係的效能
            /// </summary>
            builder.HasIndex(f => f.RequestDate)
                .HasDatabaseName("IX_Friendships_RequestDate");

            /// <summary>
            /// 建立複合唯一索引：UserId + FriendId
            /// 用途：確保兩個用戶之間只能有一個好友關係記錄，防止重複邀請
            /// </summary>
            builder.HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique()
                .HasDatabaseName("IX_Friendships_UserId_FriendId");

            /// <summary>
            /// 建立複合索引：UserId + Status + RequestDate
            /// 用途：提高查詢特定用戶特定狀態的好友關係並按時間排序的效能
            /// </summary>
            builder.HasIndex(f => new { f.UserId, f.Status, f.RequestDate })
                .HasDatabaseName("IX_Friendships_UserId_Status_RequestDate");

            /// <summary>
            /// 建立複合索引：FriendId + Status + RequestDate
            /// 用途：提高查詢特定用戶接收到的特定狀態好友邀請並按時間排序的效能
            /// </summary>
            builder.HasIndex(f => new { f.FriendId, f.Status, f.RequestDate })
                .HasDatabaseName("IX_Friendships_FriendId_Status_RequestDate");

            // === 約束配置 ===
            /// <summary>
            /// 添加檢查約束：確保 UserId 不等於 FriendId
            /// 用途：防止用戶對自己發送好友邀請
            /// </summary>
            builder.ToTable(t => t.HasCheckConstraint("CK_Friendships_NoSelfFriend", "[UserId] != [FriendId]"));

            // === 表格名稱配置 ===
            /// <summary>
            /// 設定資料庫表格名稱
            /// 用途：明確指定資料庫中的表格名稱
            /// </summary>
            builder.ToTable("Friendships");
        }
    }
}
