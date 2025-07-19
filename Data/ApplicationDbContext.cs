using Microsoft.EntityFrameworkCore;
using Matrix.Models;

namespace Matrix.Data{

    /// <summary>
    /// 應用程式資料庫上下文類別，繼承自 Entity Framework Core 的 DbContext，
    /// 用於管理資料庫連接和實體模型的配置
    /// </summary>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// 配置實體模型的關聯性、約束條件和資料庫行為
        /// </summary>
        /// <param name="modelBuilder">用於建構實體模型的模型建構器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /// <summary>
            /// 配置 ArticleHashtag 實體的複合主鍵
            /// 用途：建立文章與標籤之間的多對多關聯表，防止重複的文章標籤組合
            /// 關聯類型：多對多關聯的中間表
            /// 主鍵設定：使用 ArticleId 和 TagId 組成複合主鍵，確保每篇文章的每個標籤只能有一個關聯記錄
            /// </summary>
            modelBuilder.Entity<ArticleHashtag>()
                .HasKey(ah => new { ah.ArticleId, ah.TagId });

            /// <summary>
            /// 配置 Person 與 User 之間的一對一關聯
            /// 用途：建立使用者帳戶與個人資料之間的一對一對應關係
            /// 關聯類型：一對一關聯 (One-to-One)
            /// 外鍵設定：Person 表中的 UserId 作為外鍵，指向 User 表的主鍵
            /// 必要性：設定為必須關聯，每個個人資料都必須對應一個使用者帳戶
            /// </summary>
            modelBuilder.Entity<Person>()
                .HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<Person>(p => p.UserId)
                .IsRequired();  // 確保關聯是必須的

            /// <summary>
            /// 配置 Notification 與 Person 之間的接收者關聯
            /// 用途：建立通知與接收者之間的一對多關聯，一個人可以接收多個通知
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Notification 表中的 GetId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有通知記錄的人員資料，維護資料完整性
            /// </summary>
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(p => p.NotificationsReceived)
                .HasForeignKey(n => n.GetId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 配置 Notification 與 Person 之間的發送者關聯
            /// 用途：建立通知與發送者之間的一對多關聯，一個人可以發送多個通知
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Notification 表中的 SendId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有通知記錄的人員資料，維護資料完整性
            /// </summary>
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(p => p.NotificationsSent)
                .HasForeignKey(n => n.SendId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 配置 Report 與 Person 之間的舉報者關聯
            /// 用途：建立檢舉記錄與舉報者之間的一對多關聯，一個人可以提出多個檢舉
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Report 表中的 ReporterId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有檢舉記錄的人員資料，維護檢舉歷史記錄
            /// </summary>
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(p => p.ReportsMade)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 配置 Report 與 Person 之間的處理者關聯
            /// 用途：建立檢舉記錄與處理者之間的一對多關聯，一個人可以處理多個檢舉
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Report 表中的 ResolverId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有處理記錄的人員資料，維護檢舉處理歷史
            /// </summary>
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Resolver)
                .WithMany(p => p.ReportsResolved)
                .HasForeignKey(r => r.ResolverId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 配置 PraiseCollect 與 Person 之間的使用者關聯
            /// 用途：建立讚美收藏記錄與使用者之間的一對多關聯，一個使用者可以有多個讚美收藏
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：PraiseCollect 表中的 UserId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有讚美收藏記錄的使用者，維護使用者行為歷史
            /// </summary>
            modelBuilder.Entity<PraiseCollect>()
                .HasOne(pc => pc.User)
                .WithMany(p => p.PraiseCollects)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

            /// <summary>
            /// 配置 PraiseCollect 與 Article 之間的文章關聯
            /// 用途：建立讚美收藏記錄與文章之間的一對多關聯，一篇文章可以有多個讚美收藏
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：PraiseCollect 表中的 ArticleId 作為外鍵，指向 Article 表的主鍵
            /// 刪除行為：設定為 Cascade，當文章被刪除時，相關的讚美收藏記錄也會被自動刪除
            /// </summary>
            modelBuilder.Entity<PraiseCollect>()
                .HasOne(pc => pc.Article)
                .WithMany(a => a.PraiseCollects)
                .HasForeignKey(pc => pc.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

            /// <summary>
            /// 配置 Reply 與 Person 之間的使用者關聯
            /// 用途：建立回覆記錄與使用者之間的一對多關聯，一個使用者可以發表多個回覆
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Reply 表中的 UserId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有回覆記錄的使用者，維護回覆歷史記錄
            /// </summary>
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany(p => p.Replies)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

            /// <summary>
            /// 配置 Reply 與 Article 之間的文章關聯
            /// 用途：建立回覆記錄與文章之間的一對多關聯，一篇文章可以有多個回覆
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Reply 表中的 ArticleId 作為外鍵，指向 Article 表的主鍵
            /// 刪除行為：設定為 Cascade，當文章被刪除時，相關的回覆記錄也會被自動刪除
            /// </summary>
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Article)
                .WithMany(a => a.Replies)
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

            /// <summary>
            /// 配置 Friendship 與 Person 之間的請求者關聯
            /// 用途：建立好友關係與好友請求發起者之間的一對多關聯，一個人可以發起多個好友請求
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Friendship 表中的 UserId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有好友關係的使用者，維護好友關係完整性
            /// </summary>
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany(p => p.Friends)      // 這裡指定 Person.Friends
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 配置 Friendship 與 Person 之間的接收者關聯
            /// 用途：建立好友關係與好友請求接收者之間的一對多關聯，一個人可以接收多個好友請求
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Friendship 表中的 FriendId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有好友關係的使用者，維護好友關係完整性
            /// </summary>
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Recipient)
                .WithMany(p => p.FriendOf)     // 這裡指定 Person.FriendOf
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);
                
            /// <summary>
            /// 配置 ArticleAttachment 與 Article 之間的文章附件關聯
            /// 用途：建立文章附件與文章之間的一對多關聯，一篇文章可以有多個附件
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：ArticleAttachment 表中的 ArticleId 作為外鍵，指向 Article 表的主鍵
            /// 刪除行為：設定為 Cascade，當文章被刪除時，相關的附件記錄也會被自動刪除
            /// </summary>
            modelBuilder.Entity<ArticleAttachment>()
                .HasOne(aa => aa.Article)
                .WithMany(a => a.Attachments)
                .HasForeignKey(aa => aa.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            /// <summary>
            /// 配置 Follow 與 Person 之間的追蹤關聯
            /// 用途：建立追蹤記錄與追蹤者之間的一對多關聯，一個人可以追蹤多個對象
            /// 關聯類型：一對多關聯 (One-to-Many)
            /// 外鍵設定：Follow 表中的 UserId 作為外鍵，指向 Person 表的主鍵
            /// 刪除行為：設定為 Restrict，防止刪除有追蹤記錄的使用者，維護追蹤關係完整性
            /// 注意：FollowedId 不設定外鍵關聯，因為它可能指向不同類型的實體（如 Person、Article 等）
            /// </summary>
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User)
                .WithMany(p => p.Follows)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // FollowedId 不設定外鍵關聯，由商業邏輯處理
            // 註釋：FollowedId 可能指向不同類型的實體，例如 Person、Article 等，因此不設定外鍵關聯
        }

        /// <summary>
        /// 取得或設定使用者資料表的 DbSet
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定個人資料表的 DbSet
        /// </summary>
        public DbSet<Person> Persons { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定文章資料表的 DbSet
        /// </summary>
        public DbSet<Article> Articles { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定回覆資料表的 DbSet
        /// </summary>
        public DbSet<Reply> Replies { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定讚美收藏資料表的 DbSet
        /// </summary>
        public DbSet<PraiseCollect> PraiseCollects { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定追蹤資料表的 DbSet
        /// </summary>
        public DbSet<Follow> Follows { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定通知資料表的 DbSet
        /// </summary>
        public DbSet<Notification> Notifications { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定檢舉資料表的 DbSet
        /// </summary>
        public DbSet<Report> Reports { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定登入記錄資料表的 DbSet
        /// </summary>
        public DbSet<LoginRecord> LoginRecords { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定標籤資料表的 DbSet
        /// </summary>
        public DbSet<Hashtag> Hashtags { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定文章標籤關聯資料表的 DbSet
        /// </summary>
        public DbSet<ArticleHashtag> ArticleHashtags { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定好友關係資料表的 DbSet
        /// </summary>
        public DbSet<Friendship> Friendships { get; set; } = null!;
        
        /// <summary>
        /// 取得或設定文章附件資料表的 DbSet
        /// </summary>
        public DbSet<ArticleAttachment> ArticleAttachments { get; set; } = null!;
    }
}