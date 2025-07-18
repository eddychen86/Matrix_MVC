using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Matrix.Models;

namespace Matrix.Data.Configurations;

/// <summary>
/// User 實體的 Entity Framework Core 配置類別
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// 配置 User 實體的資料庫映射規則
    /// </summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 主鍵配置
        builder.HasKey(u => u.UserId);

        // 欄位配置
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Country)
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.Gender)
            .IsRequired(false);

        builder.Property(u => u.CreateTime)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.LastLoginTime)
            .IsRequired(false);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasDefaultValue(0);

        // 索引配置
        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("IX_Users_Status");

        builder.HasIndex(u => u.CreateTime)
            .HasDatabaseName("IX_Users_CreateTime");

        // 忽略屬性
        builder.Ignore(u => u.PasswordConfirm);

        // 表格名稱配置
        builder.ToTable("Users");
    }
}
