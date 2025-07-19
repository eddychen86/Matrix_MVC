using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Matrix.Models;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// Article 實體的 Entity Framework Core 配置類別
    ///
    /// 此檔案用於配置 Article 實體的資料庫映射設定，包括：
    /// - 主鍵配置
    /// - 欄位長度限制和驗證規則
    /// - 索引設定
    /// - 預設值設定
    /// - 欄位是否必填的約束
    ///
    /// 注意事項：
    /// - 僅能新增與 Article 實體相關的資料庫配置
    /// - 不應在此檔案中配置與其他實體的關聯性（應在 ApplicationDbContext.OnModelCreating 中處理）
    /// - 所有配置都必須符合 Article 模型的定義
    /// - 修改此檔案後需要建立新的 Migration
    /// </summary>
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        /// <summary>
        /// 配置 Article 實體的資料庫映射規則
        /// </summary>
        /// <param name="builder">實體類型建構器</param>
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            // === 主鍵配置 ===
            /// <summary>
            /// 設定 ArticleId 作為主鍵
            /// 用途：定義 Article 實體的唯一識別碼
            /// </summary>
            builder.HasKey(a => a.ArticleId);

            // === 欄位配置 ===
            /// <summary>
            /// 配置 AuthorId 欄位屬性
            /// 用途：設定文章作者的識別碼
            /// 必填：是
            /// </summary>
            builder.Property(a => a.AuthorId)
                .IsRequired();

            /// <summary>
            /// 配置 Content 欄位屬性
            /// 用途：設定文章內容的資料庫約束
            /// 長度限制：最大 4000 個字元
            /// 必填：是
            /// </summary>
            builder.Property(a => a.Content)
                .IsRequired()
                .HasMaxLength(4000);

            /// <summary>
            /// 配置 IsPublic 欄位屬性
            /// 用途：設定文章公開狀態的資料庫約束
            /// 預設值：0 （公開）
            /// 必填：是
            /// </summary>
            builder.Property(a => a.IsPublic)
                .IsRequired()
                .HasDefaultValue(0);

            /// <summary>
            /// 配置 Status 欄位屬性
            /// 用途：設定文章狀態的資料庫約束
            /// 預設值：0 （正常狀態）
            /// 必填：是
            /// </summary>
            builder.Property(a => a.Status)
                .IsRequired()
                .HasDefaultValue(0);

            /// <summary>
            /// 配置 CreateTime 欄位屬性
            /// 用途：設定文章建立時間的資料庫約束
            /// 必填：是
            /// 預設值：當前時間
            /// </summary>
            builder.Property(a => a.CreateTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            /// <summary>
            /// 配置 PraiseCount 欄位屬性
            /// 用途：設定文章讚數的資料庫約束
            /// 預設值：0
            /// 必填：是
            /// </summary>
            builder.Property(a => a.PraiseCount)
                .IsRequired()
                .HasDefaultValue(0);

            /// <summary>
            /// 配置 CollectCount 欄位屬性
            /// 用途：設定文章收藏數的資料庫約束
            /// 預設值：0
            /// 必填：是
            /// </summary>
            builder.Property(a => a.CollectCount)
                .IsRequired()
                .HasDefaultValue(0);

            // === 索引配置 ===
            /// <summary>
            /// 為 AuthorId 建立索引
            /// 用途：提高按作者查詢文章的效能
            /// </summary>
            builder.HasIndex(a => a.AuthorId)
                .HasDatabaseName("IX_Articles_AuthorId");

            /// <summary>
            /// 為 IsPublic 建立索引
            /// 用途：提高按公開狀態查詢文章的效能
            /// </summary>
            builder.HasIndex(a => a.IsPublic)
                .HasDatabaseName("IX_Articles_IsPublic");

            /// <summary>
            /// 為 Status 建立索引
            /// 用途：提高按狀態查詢文章的效能
            /// </summary>
            builder.HasIndex(a => a.Status)
                .HasDatabaseName("IX_Articles_Status");

            /// <summary>
            /// 為 CreateTime 建立索引
            /// 用途：提高按建立時間查詢和排序文章的效能
            /// </summary>
            builder.HasIndex(a => a.CreateTime)
                .HasDatabaseName("IX_Articles_CreateTime");

            /// <summary>
            /// 為 PraiseCount 建立索引
            /// 用途：提高按讚數排序文章的效能
            /// </summary>
            builder.HasIndex(a => a.PraiseCount)
                .HasDatabaseName("IX_Articles_PraiseCount");

            /// <summary>
            /// 為 CollectCount 建立索引
            /// 用途：提高按收藏數排序文章的效能
            /// </summary>
            builder.HasIndex(a => a.CollectCount)
                .HasDatabaseName("IX_Articles_CollectCount");

            /// <summary>
            /// 建立複合索引：AuthorId + CreateTime
            /// 用途：提高查詢特定作者按時間排序的文章效能
            /// </summary>
            builder.HasIndex(a => new { a.AuthorId, a.CreateTime })
                .HasDatabaseName("IX_Articles_AuthorId_CreateTime");

            /// <summary>
            /// 建立複合索引：IsPublic + Status + CreateTime
            /// 用途：提高查詢公開且正常狀態的文章並按時間排序的效能
            /// </summary>
            builder.HasIndex(a => new { a.IsPublic, a.Status, a.CreateTime })
                .HasDatabaseName("IX_Articles_IsPublic_Status_CreateTime");

            // === 表格名稱配置 ===
            /// <summary>
            /// 設定資料庫表格名稱
            /// 用途：明確指定資料庫中的表格名稱
            /// </summary>
            builder.ToTable("Articles");
        }
    }
}
