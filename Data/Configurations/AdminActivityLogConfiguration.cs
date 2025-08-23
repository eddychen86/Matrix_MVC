using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// AdminActivityLog 實體的 Entity Framework Core 配置類別
    ///
    /// 此檔案用於配置 AdminActivityLog 實體的資料庫映射設定，包括：
    /// - 主鍵配置
    /// - 欄位長度限制和驗證規則
    /// - 索引設定
    /// - 預設值設定
    /// - 欄位是否必填的約束
    /// - 與 Person 實體的關聯配置
    ///
    /// 注意事項：
    /// - 此配置針對管理員活動記錄的擴展欄位
    /// - 保持向後相容性，表名維持 "LoginRecords"
    /// - 主鍵維持原有的 LoginId 名稱
    /// - 修改此檔案後需要建立新的 Migration
    /// </summary>
    public class AdminActivityLogConfiguration : IEntityTypeConfiguration<AdminActivityLog>
    {
        /// <summary>
        /// 配置 AdminActivityLog 實體的資料庫映射規則
        /// </summary>
        /// <param name="builder">實體類型建構器</param>
        public void Configure(EntityTypeBuilder<AdminActivityLog> builder)
        {
            // === 表格名稱配置 ===
            /// <summary>
            /// 設定資料庫表格名稱
            /// 用途：保持向後相容性，維持原有的 LoginRecords 表名
            /// </summary>
            builder.ToTable("LoginRecords");

            // === 主鍵配置 ===
            /// <summary>
            /// 設定 LoginId 作為主鍵
            /// 用途：定義 AdminActivityLog 實體的唯一識別碼
            /// 注意：保持原有的 LoginId 名稱以維持向後相容性
            /// </summary>
            builder.HasKey(a => a.LoginId);

            builder.Property(a => a.LoginId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // === 關聯配置 ===
            /// <summary>
            /// 設定 AdminActivityLog 與 Person 的多對一關聯
            /// 用途：使用 AdminActivityLog.UserId 作為外鍵關聯到 Person
            /// </summary>
            builder.HasOne(a => a.User)
                .WithMany(p => p.LoginRecords)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict) // 防止級聯刪除，保留活動記錄
                .IsRequired();

            // === 基本欄位配置 ===
            /// <summary>
            /// 配置 UserId 欄位屬性
            /// 用途：設定關聯使用者的識別碼
            /// 必填：是
            /// </summary>
            builder.Property(a => a.UserId)
                .IsRequired();

            /// <summary>
            /// 配置 AdminName 欄位屬性
            /// 用途：設定管理員名稱
            /// 長度限制：最大 50 個字元
            /// 必填：是
            /// </summary>
            builder.Property(a => a.AdminName)
                .IsRequired()
                .HasMaxLength(50);

            /// <summary>
            /// 配置 Role 欄位屬性
            /// 用途：設定管理員角色等級 (1=管理員, 2=超級管理員)
            /// 必填：是
            /// 預設值：1 (一般管理員)
            /// </summary>
            builder.Property(a => a.Role)
                .IsRequired()
                .HasDefaultValue(1);

            // === 網路資訊欄位配置 ===
            /// <summary>
            /// 配置 IpAddress 欄位屬性
            /// 用途：設定活動發生時的 IP 地址
            /// 長度限制：最大 45 個字元 (支援 IPv6)
            /// 必填：是
            /// </summary>
            builder.Property(a => a.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            /// <summary>
            /// 配置 UserAgent 欄位屬性
            /// 用途：設定瀏覽器和設備資訊
            /// 長度限制：最大 500 個字元
            /// 必填：是
            /// </summary>
            builder.Property(a => a.UserAgent)
                .IsRequired()
                .HasMaxLength(500);

            // === 時間欄位配置 ===
            /// <summary>
            /// 配置 LoginTime 欄位屬性
            /// 用途：設定登入時間 (Session 開始時間)
            /// 必填：否
            /// </summary>
            builder.Property(a => a.LoginTime)
                .IsRequired(false);

            /// <summary>
            /// 配置 LogoutTime 欄位屬性
            /// 用途：設定登出時間 (Session 結束時間)
            /// 必填：否
            /// </summary>
            builder.Property(a => a.LogoutTime)
                .IsRequired(false);

            /// <summary>
            /// 配置 ActionTime 欄位屬性
            /// 用途：設定活動發生時間
            /// 必填：是
            /// 預設值：當前時間
            /// </summary>
            builder.Property(a => a.ActionTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // === 活動資訊欄位配置 ===
            /// <summary>
            /// 配置 PagePath 欄位屬性
            /// 用途：設定頁面路徑
            /// 長度限制：最大 500 個字元
            /// 必填：否
            /// </summary>
            builder.Property(a => a.PagePath)
                .HasMaxLength(500);

            /// <summary>
            /// 配置 ActionType 欄位屬性
            /// 用途：設定操作類型 (LOGIN, LOGOUT, VIEW, CREATE, UPDATE, DELETE, ERROR)
            /// 長度限制：最大 20 個字元
            /// 必填：是
            /// </summary>
            builder.Property(a => a.ActionType)
                .IsRequired()
                .HasMaxLength(20);

            /// <summary>
            /// 配置 ActionDescription 欄位屬性
            /// 用途：設定活動描述
            /// 長度限制：最大 1000 個字元
            /// 必填：是
            /// </summary>
            builder.Property(a => a.ActionDescription)
                .IsRequired()
                .HasMaxLength(1000);

            /// <summary>
            /// 配置 Duration 欄位屬性
            /// 用途：設定在頁面停留的時間 (秒)
            /// 必填：是
            /// 預設值：0
            /// </summary>
            builder.Property(a => a.Duration)
                .IsRequired()
                .HasDefaultValue(0);

            // === 狀態欄位配置 ===
            /// <summary>
            /// 配置 IsSuccessful 欄位屬性
            /// 用途：設定操作是否成功
            /// 必填：是
            /// 預設值：true
            /// </summary>
            builder.Property(a => a.IsSuccessful)
                .IsRequired()
                .HasDefaultValue(true);

            /// <summary>
            /// 配置 ErrorMessage 欄位屬性
            /// 用途：設定錯誤訊息 (當 IsSuccessful = false 時)
            /// 長度限制：最大 1000 個字元
            /// 必填：否
            /// </summary>
            builder.Property(a => a.ErrorMessage)
                .HasMaxLength(1000);

            // === 索引配置 ===
            /// <summary>
            /// 為 UserId 建立索引
            /// 用途：提高按用戶查詢活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_LoginRecords_UserId");

            /// <summary>
            /// 為 ActionTime 建立索引
            /// 用途：提高按時間查詢和排序活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.ActionTime)
                .HasDatabaseName("IX_LoginRecords_ActionTime");

            /// <summary>
            /// 為 ActionType 建立索引
            /// 用途：提高按操作類型查詢活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.ActionType)
                .HasDatabaseName("IX_LoginRecords_ActionType");

            /// <summary>
            /// 為 IpAddress 建立索引
            /// 用途：提高按 IP 地址查詢活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.IpAddress)
                .HasDatabaseName("IX_LoginRecords_IpAddress");

            /// <summary>
            /// 為 IsSuccessful 建立索引
            /// 用途：提高按成功狀態查詢活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.IsSuccessful)
                .HasDatabaseName("IX_LoginRecords_IsSuccessful");

            /// <summary>
            /// 為 PagePath 建立索引
            /// 用途：提高按頁面路徑查詢活動記錄的效能
            /// </summary>
            builder.HasIndex(a => a.PagePath)
                .HasDatabaseName("IX_LoginRecords_PagePath");

            /// <summary>
            /// 建立複合索引：UserId + ActionTime
            /// 用途：優化按用戶和時間範圍查詢的效能
            /// </summary>
            builder.HasIndex(a => new { a.UserId, a.ActionTime })
                .HasDatabaseName("IX_LoginRecords_UserId_ActionTime");

            /// <summary>
            /// 建立複合索引：ActionType + ActionTime
            /// 用途：優化按操作類型和時間範圍查詢的效能
            /// </summary>
            builder.HasIndex(a => new { a.ActionType, a.ActionTime })
                .HasDatabaseName("IX_LoginRecords_ActionType_ActionTime");
        }
    }
}