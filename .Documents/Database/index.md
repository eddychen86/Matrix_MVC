# 資料庫技術文件索引

**分類**: Database Development  
**技術領域**: 資料持久化、ORM 技術、資料存取層設計  

## 📋 文件列表

### 文件 1: Entity Framework Core 完整指南
**檔案**: [`entity-framework.md`](./entity-framework.md)  
**描述**: EF Core 8.0 深度解析，涵蓋 Code First、Migration、Repository 模式等進階技術  
**關鍵字**: Entity Framework Core, ORM, Code First, Migration, Repository Pattern, 查詢優化  
**相關檔案**: Data/ApplicationDbContext.cs, Models/, Migrations/, Repository/  
**複雜度**: 中級到高級  

**內容概要**:
- DbContext 設定與管理
- Fluent API 實體配置
- Migration 版本控制
- Repository 模式實作
- 查詢優化技術
- 併發控制機制
- 效能調優策略

---

## 🎯 學習路線

### 入門階段 (1-2 週)
1. **ORM 概念**: 理解物件關聯對映基礎理論
2. **DbContext**: 學習資料庫上下文的設定與使用
3. **基本操作**: 掌握 CRUD 操作的實作方法

### 進階階段 (2-4 週)  
1. **Fluent API**: 學習進階的實體配置技術
2. **Migration**: 掌握資料庫版本控制和結構變更
3. **關聯關係**: 理解一對一、一對多、多對多關係設定

### 專家階段 (2-3 週)
1. **Repository 模式**: 實作資料存取層抽象
2. **效能優化**: 學習查詢優化和快取策略
3. **併發控制**: 掌握樂觀/悲觀鎖定機制

---

## 🔗 技術關聯

### 直接相關技術
- **SQL Server**: 後端資料庫系統
- **ASP.NET Core**: 業務邏輯層整合
- **AutoMapper**: 物件對映工具

### 整合技術
- **Memory Cache**: 第二層快取
- **Redis**: 分散式快取
- **Azure SQL**: 雲端資料庫服務

---

## 📊 重點技術領域

### 資料模型設計
- **實體關係**: User ↔ Person (一對一)
- **階層關係**: Article → Reply (一對多)
- **多對多關係**: Article ↔ Hashtag
- **軟刪除**: IsDeleted 欄位設計

### 查詢最佳化
- **Include 預載**: 避免 N+1 查詢問題
- **投影查詢**: 減少資料傳輸量
- **分頁處理**: Skip/Take 效能優化
- **索引設計**: 提升查詢效能

### 併發處理
- **樂觀併發**: RowVersion/Timestamp
- **事務管理**: BeginTransaction/Commit/Rollback
- **隔離等級**: 避免髒讀/幻讀
- **死鎖處理**: 重試機制設計

---

## 💡 實作重點

### 開發準備
- **資料庫工具**: SQL Server Management Studio 或 Azure Data Studio
- **Migration 工具**: dotnet ef 命令列工具
- **監控工具**: SQL Profiler 或 Application Insights

### 常見模式
```csharp
// Repository 介面設計
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
}

// 實體配置範例  
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(20);
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
```

### 效能最佳化
- **連線池管理**: 適當的 DbContext 生命週期
- **非同步操作**: 使用 async/await 避免阻塞
- **查詢分離**: 讀寫分離提升效能
- **快取策略**: 記憶體快取常用資料

---

## 📚 推薦學習資源

### 官方文件
- [EF Core 官方文檔](https://docs.microsoft.com/ef/core/)
- [Migration 指南](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [效能最佳實務](https://docs.microsoft.com/ef/core/performance/)

### 實作範例
- Matrix 專案中的實際程式碼
- Repository 模式實作範例
- 複雜查詢最佳化案例

---

**最後更新**: 2025-08-29  
**文件數量**: 1  
**總學習時間**: 5-9 週 (依個人基礎而定)