# Entity Framework Core 技術文件

**技術分類**: 資料存取層 (ORM)  
**複雜度**: 中級到高級  
**適用情境**: .NET 應用程式資料庫操作、Code First 開發模式  

## 技術概述

Matrix 專案使用 Entity Framework Core 8.0 作為 ORM (Object-Relational Mapping) 框架，採用 Code First 方法管理資料庫結構，並透過 Migration 機制進行版本控制。

## 基礎技術

### 1. 核心組件架構
```
Data/
├── ApplicationDbContext.cs           # 資料庫上下文
├── Configurations/                   # 實體設定
│   ├── UserConfiguration.cs         # 用戶實體設定
│   ├── ArticleConfiguration.cs      # 文章實體設定
│   └── PersonConfiguration.cs       # 個人檔案設定
Models/                              # 實體模型
├── User.cs                         # 用戶模型
├── Article.cs                      # 文章模型
├── Person.cs                       # 個人檔案模型
└── [Other Models]                  # 其他業務模型
Migrations/                         # 資料庫遷移
├── 20250717174655_InitialCreate.cs # 初始建立
└── [Other Migrations]              # 其他遷移檔案
```

### 2. 資料庫上下文設定 (ApplicationDbContext.cs:1-35)
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet 定義
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Reply> Replies { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Follow> Follows { get; set; } = null!;
    // ... 其他 DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 套用實體設定
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());
        // ... 其他設定
    }
}
```

### 3. 連線設定 (Program.cs:55-60)
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60); // 命令超時設定
        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null); // 重試機制
    }));
```

## 進階技術

### 1. Fluent API 實體設定
```csharp
// UserConfiguration.cs 範例
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 表名設定
        builder.ToTable("Users");

        // 主鍵設定
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId)
               .HasDefaultValueSql("NEWID()"); // GUID 預設值

        // 屬性設定
        builder.Property(u => u.UserName)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(255);

        // 索引設定
        builder.HasIndex(u => u.UserName)
               .IsUnique()
               .HasDatabaseName("IX_Users_UserName");

        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("IX_Users_Email");

        // 關聯設定
        builder.HasOne(u => u.Person)
               .WithOne(p => p.User)
               .HasForeignKey<Person>(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // 軟刪除設定
        builder.Property(u => u.IsDeleted)
               .HasDefaultValue(false);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
```

### 2. 複雜關聯關係設定
```csharp
// ArticleConfiguration.cs - 多對多關係範例
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // 一對多關係：Article -> Replies
        builder.HasMany(a => a.Replies)
               .WithOne(r => r.Article)
               .HasForeignKey(r => r.ArticleId)
               .OnDelete(DeleteBehavior.Cascade);

        // 多對多關係：Article <-> Hashtag
        builder.HasMany(a => a.ArticleHashtags)
               .WithOne(ah => ah.Article)
               .HasForeignKey(ah => ah.ArticleId);

        // 併發控制
        builder.Property(a => a.RowVersion)
               .IsRowVersion();

        // JSON 欄位設定 (EF Core 8.0+)
        builder.OwnsOne(a => a.Metadata, metadata =>
        {
            metadata.ToJson();
            metadata.Property(m => m.Tags);
            metadata.Property(m => m.Category);
        });
    }
}
```

### 3. 查詢篩選器與軟刪除
```csharp
// 全域查詢篩選器
builder.HasQueryFilter(u => !u.IsDeleted);

// 實體基底類別
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

// 軟刪除實作
public class SoftDeleteService
{
    public async Task SoftDeleteAsync<T>(T entity) where T : BaseEntity
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

## Migration 管理

### 1. Migration 命令
```bash
# 新增 Migration
dotnet ef migrations add MigrationName

# 更新資料庫
dotnet ef database update

# 移除最後一個 Migration
dotnet ef migrations remove

# 查看 Migration 狀態
dotnet ef migrations list

# 產生 SQL 腳本
dotnet ef migrations script
```

### 2. Migration 檔案結構
```csharp
// 20250717174655_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserId = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                UserName = table.Column<string>(maxLength: 20, nullable: false),
                Email = table.Column<string>(maxLength: 30, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                IsDeleted = table.Column<bool>(nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_UserName",
            table: "Users",
            column: "UserName",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

### 3. 資料種子設定
```csharp
// 在 OnModelCreating 中設定初始資料
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 種子資料
    modelBuilder.Entity<Role>().HasData(
        new Role { RoleId = Guid.NewGuid(), RoleName = "Admin" },
        new Role { RoleId = Guid.NewGuid(), RoleName = "User" },
        new Role { RoleId = Guid.NewGuid(), RoleName = "Moderator" }
    );
}
```

## Repository 模式實作

### 1. 泛型 Repository 介面
```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<bool> ExistsAsync(object id);
}
```

### 2. Repository 基底實作
```csharp
// BaseRepository.cs
public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}
```

### 3. 特定 Repository 實作
```csharp
// UserRepository.cs
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await _dbSet
            .AnyAsync(u => u.UserName == username || u.Email == email);
    }

    public async Task<IEnumerable<User>> GetUsersWithPostCountAsync()
    {
        return await _dbSet
            .Include(u => u.Person)
            .Include(u => u.Articles)
            .Select(u => new User
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Person = u.Person,
                PostCount = u.Articles.Count
            })
            .ToListAsync();
    }
}
```

## 效能優化技術

### 1. 查詢最佳化
```csharp
// 避免 N+1 查詢問題
public async Task<IEnumerable<Article>> GetArticlesWithAuthorsAsync()
{
    return await _context.Articles
        .Include(a => a.Person)      // 預載關聯資料
        .Include(a => a.Replies)
            .ThenInclude(r => r.Person)
        .ToListAsync();
}

// 投影查詢減少資料傳輸
public async Task<IEnumerable<ArticleDto>> GetArticleSummariesAsync()
{
    return await _context.Articles
        .Select(a => new ArticleDto
        {
            ArticleId = a.ArticleId,
            Title = a.Title,
            AuthorName = a.Person.DisplayName,
            CreatedAt = a.CreatedAt
        })
        .ToListAsync();
}

// 分頁查詢
public async Task<PagedResult<Article>> GetArticlesPagedAsync(int page, int pageSize)
{
    var totalCount = await _context.Articles.CountAsync();
    
    var articles = await _context.Articles
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Include(a => a.Person)
        .ToListAsync();

    return new PagedResult<Article>
    {
        Items = articles,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### 2. 非同步操作與並行處理
```csharp
// 平行執行多個查詢
public async Task<DashboardData> GetDashboardDataAsync()
{
    var userCountTask = _context.Users.CountAsync();
    var articleCountTask = _context.Articles.CountAsync();
    var recentArticlesTask = _context.Articles
        .OrderByDescending(a => a.CreatedAt)
        .Take(10)
        .ToListAsync();

    await Task.WhenAll(userCountTask, articleCountTask, recentArticlesTask);

    return new DashboardData
    {
        UserCount = await userCountTask,
        ArticleCount = await articleCountTask,
        RecentArticles = await recentArticlesTask
    };
}
```

### 3. 快取策略
```csharp
// 第二層快取實作
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public CachedUserRepository(IUserRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<User?> GetByIdAsync(object id)
    {
        var cacheKey = $"user_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out User user))
        {
            user = await _repository.GetByIdAsync(id);
            if (user != null)
            {
                _cache.Set(cacheKey, user, _cacheExpiration);
            }
        }
        
        return user;
    }

    // 清除快取
    public async Task UpdateAsync(User entity)
    {
        await _repository.UpdateAsync(entity);
        _cache.Remove($"user_{entity.UserId}");
    }
}
```

## 實際應用情境

### 1. 併發控制處理
```csharp
// 樂觀併發控制
public async Task<bool> UpdateArticleAsync(Guid articleId, string title, byte[] rowVersion)
{
    try
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
        if (article == null) return false;

        // 檢查 RowVersion
        if (!article.RowVersion.SequenceEqual(rowVersion))
        {
            throw new DbUpdateConcurrencyException("資料已被其他用戶修改");
        }

        article.Title = title;
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
    catch (DbUpdateConcurrencyException)
    {
        // 處理併發衝突
        throw;
    }
}
```

### 2. 事務處理
```csharp
// 複雜業務邏輯事務
public async Task<bool> CreatePostWithNotificationAsync(CreatePostDto dto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. 建立文章
        var article = new Article
        {
            Title = dto.Title,
            Content = dto.Content,
            PersonId = dto.AuthorId
        };
        
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // 2. 處理標籤
        foreach (var tagName in dto.Tags)
        {
            var hashtag = await _context.Hashtags
                .FirstOrDefaultAsync(h => h.TagName == tagName);
                
            if (hashtag == null)
            {
                hashtag = new Hashtag { TagName = tagName };
                _context.Hashtags.Add(hashtag);
                await _context.SaveChangesAsync();
            }

            _context.ArticleHashtags.Add(new ArticleHashtag
            {
                ArticleId = article.ArticleId,
                HashtagId = hashtag.HashtagId
            });
        }

        // 3. 建立通知給追蹤者
        var followers = await _context.Follows
            .Where(f => f.FollowingId == dto.AuthorId)
            .Select(f => f.FollowerId)
            .ToListAsync();

        foreach (var followerId in followers)
        {
            _context.Notifications.Add(new Notification
            {
                RecipientId = followerId,
                SenderId = dto.AuthorId,
                Type = NotificationType.NewPost,
                ReferenceId = article.ArticleId,
                Message = "發布了新文章"
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return true;
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 3. 複雜查詢實作
```csharp
// 搜尋功能實作
public async Task<IEnumerable<Article>> SearchArticlesAsync(string keyword, int userId)
{
    var query = _context.Articles
        .Include(a => a.Person)
        .Include(a => a.ArticleHashtags)
            .ThenInclude(ah => ah.Hashtag)
        .AsQueryable();

    if (!string.IsNullOrEmpty(keyword))
    {
        query = query.Where(a => 
            a.Title.Contains(keyword) ||
            a.Content.Contains(keyword) ||
            a.ArticleHashtags.Any(ah => ah.Hashtag.TagName.Contains(keyword))
        );
    }

    // 依用戶偏好排序
    var userPreferences = await GetUserPreferencesAsync(userId);
    
    if (userPreferences.SortByPopularity)
    {
        query = query.OrderByDescending(a => 
            a.PraiseCollects.Count() + a.Replies.Count());
    }
    else
    {
        query = query.OrderByDescending(a => a.CreatedAt);
    }

    return await query
        .Take(50)
        .ToListAsync();
}
```

---

**建立日期**: 2025-08-29  
**適用版本**: Entity Framework Core 8.0  
**相關檔案**: Data/, Models/, Migrations/  
**資料庫**: SQL Server  
**學習資源**: [EF Core 官方文檔](https://docs.microsoft.com/ef/core/)