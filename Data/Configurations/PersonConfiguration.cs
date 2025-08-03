using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// Person 實體的 Entity Framework Core 配置類別
    ///
    /// 此檔案用於配置 Person 實體的資料庫映射設定，包括：
    /// - 主鍵配置
    /// - 欄位長度限制和驗證規則
    /// - 索引設定
    /// - 預設值設定
    /// - 欄位是否必填的約束
    ///
    /// 注意事項：
    /// - 僅能新增與 Person 實體相關的資料庫配置
    /// - 不應在此檔案中配置與其他實體的關聯性（應在 ApplicationDbContext.OnModelCreating 中處理）
    /// - 所有配置都必須符合 Person 模型的定義
    /// - 修改此檔案後需要建立新的 Migration
    /// </summary>
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        /// <summary>
        /// 配置 Person 實體的資料庫映射規則
        /// </summary>
        /// <param name="builder">實體類型建構器</param>
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            // === 主鍵配置 ===
            /// <summary>
            /// 設定 PersonId 作為主鍵
            /// 用途：定義 Person 實體的唯一識別碼
            /// </summary>
            builder.HasKey(p => p.PersonId);

            builder.Property(p => p.PersonId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // === 欄位配置 ===
            /// <summary>
            /// 配置 UserId 欄位屬性
            /// 用途：設定關聯使用者的識別碼
            /// 必填：是
            /// 唯一性：建立唯一索引以確保一對一關聯
            /// </summary>
            builder.Property(p => p.UserId)
                .IsRequired();

            /// <summary>
            /// 配置 DisplayName 欄位屬性
            /// 用途：設定顯示名稱的資料庫約束
            /// 長度限制：最大 50 個字元
            /// 必填：否
            /// </summary>
            builder.Property(p => p.DisplayName)
                .HasMaxLength(50);

            /// <summary>
            /// 配置 Bio 欄位屬性
            /// 用途：設定個人簡介的資料庫約束
            /// 長度限制：最大 300 個字元
            /// 必填：否
            /// </summary>
            builder.Property(p => p.Bio)
                .HasMaxLength(300);

            /// <summary>
            /// 配置 AvatarPath 欄位屬性
            /// 用途：設定頭像二進制資料的資料庫約束
            /// 類型：byte[]
            /// 必填：否
            /// </summary>
            builder.Property(p => p.AvatarPath);

            /// <summary>
            /// 配置 BannerPath 欄位屬性
            /// 用途：設定橫幅二進制資料的資料庫約束
            /// 類型：byte[]
            /// 必填：否
            /// </summary>
            builder.Property(p => p.BannerPath);

            /// <summary>
            /// 配置 Website 欄位屬性
            /// 用途：設定外部網站連結的資料庫約束
            /// 長度限制：最大 2048 個字元
            /// 必填：否
            /// </summary>
            builder.Property(p => p.Website1)
                .HasMaxLength(2048);
            
            builder.Property(p => p.Website2)
                .HasMaxLength(2048);
                
            builder.Property(p => p.Website3)
                .HasMaxLength(2048);

            /// <summary>
            /// 配置 IsPrivate 欄位屬性
            /// 用途：設定隱私設定的資料庫約束
            /// 預設值：0 （公開）
            /// 必填：是
            /// </summary>
            builder.Property(p => p.IsPrivate)
                .IsRequired()
                .HasDefaultValue(0);

            /// <summary>
            /// 配置 WalletAddress 欄位屬性
            /// 用途：設定區塊鏈錢包地址的資料庫約束
            /// 長度限制：最大 100 個字元
            /// 必填：否
            /// </summary>
            builder.Property(p => p.WalletAddress)
                .HasMaxLength(100);

            /// <summary>
            /// 配置 ModifyTime 欄位屬性
            /// 用途：設定個人資料修改時間的資料庫約束
            /// 必填：否
            /// </summary>
            builder.Property(p => p.ModifyTime)
                .IsRequired(false);

            // === 索引配置 ===
            /// <summary>
            /// 為 UserId 建立唯一索引
            /// 用途：確保與 User 實體的一對一關聯，提高查詢效能
            /// </summary>
            builder.HasIndex(p => p.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Persons_UserId");

            /// <summary>
            /// 為 DisplayName 建立索引
            /// 用途：提高按顯示名稱查詢的效能
            /// </summary>
            builder.HasIndex(p => p.DisplayName)
                .HasDatabaseName("IX_Persons_DisplayName");

            /// <summary>
            /// 為 IsPrivate 建立索引
            /// 用途：提高按隱私設定查詢的效能
            /// </summary>
            builder.HasIndex(p => p.IsPrivate)
                .HasDatabaseName("IX_Persons_IsPrivate");

            /// <summary>
            /// 為 WalletAddress 建立索引
            /// 用途：提高按錢包地址查詢的效能
            /// </summary>
            builder.HasIndex(p => p.WalletAddress)
                .HasDatabaseName("IX_Persons_WalletAddress");

            /// <summary>
            /// 為 ModifyTime 建立索引
            /// 用途：提高按修改時間查詢和排序的效能
            /// </summary>
            builder.HasIndex(p => p.ModifyTime)
                .HasDatabaseName("IX_Persons_ModifyTime");

            // === 表格名稱配置 ===
            /// <summary>
            /// 設定資料庫表格名稱
            /// 用途：明確指定資料庫中的表格名稱
            /// </summary>
            builder.ToTable("Persons");
        }
    }
}
