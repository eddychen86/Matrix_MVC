using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Matrix.Models;

namespace Matrix.Data.Configurations;

/// <summary>
/// User 實體的 Entity Framework Core 配置類別
/// 
/// 此檔案用於配置 User 實體的資料庫映射設定，包括：
/// - 主鍵配置
/// - 欄位長度限制和驗證規則
/// - 索引設定
/// - 預設值設定
/// - 欄位是否必填的約束
/// 
/// 注意事項：
/// - 僅能新增與 User 實體相關的資料庫配置
/// - 不應在此檔案中配置與其他實體的關聯性（應在 ApplicationDbContext.OnModelCreating 中處理）
/// - 所有配置都必須符合 User 模型的定義
/// - 修改此檔案後需要建立新的 Migration
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// 配置 User 實體的資料庫映射規則
    /// </summary>
    /// <param name="builder">實體類型建構器</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // === 主鍵配置 ===
        /// <summary>
        /// 設定 UserId 作為主鍵
        /// 用途：定義 User 實體的唯一識別碼
        /// </summary>
        builder.HasKey(u => u.UserId);

        // === 欄位配置 ===
        /// <summary>
        /// 配置 UserName 欄位屬性
        /// 用途：設定使用者名稱的資料庫約束
        /// 長度限制：最大 50 個字元
        /// 必填：是
        /// 唯一性：建立唯一索引以確保使用者名稱不重複
        /// </summary>
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        /// <summary>
        /// 配置 Email 欄位屬性
        /// 用途：設定電子郵件地址的資料庫約束
        /// 長度限制：最大 100 個字元
        /// 必填：是
        /// 唯一性：建立唯一索引以確保電子郵件地址不重複
        /// </summary>
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        /// <summary>
        /// 配置 Password 欄位屬性
        /// 用途：設定密碼的資料庫約束
        /// 長度限制：最大 255 個字元（考慮加密後的長度）
        /// 必填：是
        /// </summary>
        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        /// <summary>
        /// 配置 Country 欄位屬性
        /// 用途：設定國家資訊的資料庫約束
        /// 長度限制：最大 100 個字元
        /// 必填：否
        /// </summary>
        builder.Property(u => u.Country)
            .HasMaxLength(100);

        /// <summary>
        /// 配置 Role 欄位屬性
        /// 用途：設定使用者權限等級的資料庫約束
        /// 預設值：0 （一般使用者）
        /// 必填：是
        /// </summary>
        builder.Property(u => u.Role)
            .IsRequired()
            .HasDefaultValue(0);

        /// <summary>
        /// 配置 Gender 欄位屬性
        /// 用途：設定性別資訊的資料庫約束
        /// 必填：否
        /// </summary>
        builder.Property(u => u.Gender)
            .IsRequired(false);

        /// <summary>
        /// 配置 CreateTime 欄位屬性
        /// 用途：設定帳號建立時間的資料庫約束
        /// 必填：是
        /// 預設值：當前時間
        /// </summary>
        builder.Property(u => u.CreateTime)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        /// <summary>
        /// 配置 LastLoginTime 欄位屬性
        /// 用途：設定最後登入時間的資料庫約束
        /// 必填：否
        /// </summary>
        builder.Property(u => u.LastLoginTime)
            .IsRequired(false);

        /// <summary>
        /// 配置 Status 欄位屬性
        /// 用途：設定帳號狀態的資料庫約束
        /// 預設值：0 （啟用狀態）
        /// 必填：是
        /// </summary>
        builder.Property(u => u.Status)
            .IsRequired()
            .HasDefaultValue(0);

        // === 索引配置 ===
        /// <summary>
        /// 為 UserName 建立唯一索引
        /// 用途：確保使用者名稱的唯一性，提高查詢效能
        /// </summary>
        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        /// <summary>
        /// 為 Email 建立唯一索引
        /// 用途：確保電子郵件地址的唯一性，提高查詢效能
        /// </summary>
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        /// <summary>
        /// 為 Status 建立索引
        /// 用途：提高按狀態查詢使用者的效能
        /// </summary>
        builder.HasIndex(u => u.Status)
            .HasDatabaseName("IX_Users_Status");

        /// <summary>
        /// 為 CreateTime 建立索引
        /// 用途：提高按建立時間查詢和排序的效能
        /// </summary>
        builder.HasIndex(u => u.CreateTime)
            .HasDatabaseName("IX_Users_CreateTime");

        // === 忽略屬性 ===
        /// <summary>
        /// 忽略 PasswordConfirm 屬性
        /// 用途：此屬性僅用於表單驗證，不需要儲存到資料庫
        /// </summary>
        builder.Ignore(u => u.PasswordConfirm);

        // === 表格名稱配置 ===
        /// <summary>
        /// 設定資料庫表格名稱
        /// 用途：明確指定資料庫中的表格名稱
        /// </summary>
        builder.ToTable("Users");
    }
}
