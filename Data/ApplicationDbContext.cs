using Matrix.Data.Configurations;

namespace Matrix.Data
{

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
            /// 設定 ArticleHashtag 的複合主鍵，確保文章和標籤間多對多關聯的唯一性
            /// </summary>
            modelBuilder
                .Entity<ArticleHashtag>()
                .HasKey(ah => new { ah.ArticleId, ah.TagId });

            /// <summary>
            /// 設定 Person 與 User 的一對一關聯，使用 Person.UserId 作為外鍵並設為必要關聯
            /// </summary>
            modelBuilder.Entity<Person>()
                .HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<Person>(p => p.UserId)
                .IsRequired();  // 確保關聯是必須的

            /// <summary>
            /// 設定 Article 與 Person 的一對多關聯，一個 Person 可以有多篇 Article，使用 Article.AuthorId 作為外鍵
            /// </summary>
            modelBuilder.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(p => p.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定通知接收者關聯，一人可接收多個通知，使用 GetId 作為外鍵，限制刪除以維護資料完整性
            /// </summary>
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(p => p.NotificationsReceived)
                .HasForeignKey(n => n.GetId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定通知發送者關聯，一人可發送多個通知，使用 SendId 作為外鍵，限制刪除以維護資料完整性
            /// </summary>
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(p => p.NotificationsSent)
                .HasForeignKey(n => n.SendId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定舉報者關聯，一人可提出多個檢舉，使用 ReporterId 作為外鍵，限制刪除以保留舉報歷史
            /// </summary>
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(p => p.ReportsMade)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定檢舉處理者關聯，一人可處理多個檢舉，使用 ResolverId 作為外鍵，限制刪除以保留處理歷史
            /// </summary>
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Resolver)
                .WithMany(p => p.ReportsResolved)
                .HasForeignKey(r => r.ResolverId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定使用者讚美收藏關聯，一人可有多個讚美收藏，使用 UserId 作為外鍵，限制刪除以保留用戶行為
            /// </summary>
            modelBuilder.Entity<PraiseCollect>()
                .HasOne(pc => pc.User)
                .WithMany(p => p.PraiseCollects)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

            /// <summary>
            /// 設定文章讚美收藏關聯，一篇文章可有多個讚美收藏，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            modelBuilder.Entity<PraiseCollect>()
                .HasOne(pc => pc.Article)
                .WithMany(a => a.PraiseCollects)
                .HasForeignKey(pc => pc.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

            /// <summary>
            /// 設定使用者回覆關聯，一人可發表多個回覆，使用 UserId 作為外鍵，限制刪除以保留回覆歷史
            /// </summary>
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany(p => p.Replies)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

            /// <summary>
            /// 設定文章回覆關聯，一篇文章可有多個回覆，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Article)
                .WithMany(a => a.Replies)
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

            /// <summary>
            /// 設定好友請求發起者關聯，一人可發起多個好友請求，使用 UserId 作為外鍵，限制刪除以維護關係完整性
            /// </summary>
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany(p => p.Friends)      // 這裡指定 Person.Friends
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定好友請求接收者關聯，一人可接收多個好友請求，使用 FriendId 作為外鍵，限制刪除以維護關係完整性
            /// </summary>
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Recipient)
                .WithMany(p => p.FriendOf)     // 這裡指定 Person.FriendOf
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// 設定文章附件關聯，一篇文章可有多個附件，使用 ArticleId 作為外鍵，刪除文章時級聯刪除
            /// </summary>
            modelBuilder.Entity<ArticleAttachment>()
                .HasOne(aa => aa.Article)
                .WithMany(a => a.Attachments)
                .HasForeignKey(aa => aa.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            /// <summary>
            /// 設定追蹤者關聯，一人可追蹤多個對象，使用 UserId 作為外鍵，限制刪除以維護追蹤關係
            /// FollowedId 不設外鍵，可指向不同實體類型
            /// </summary>
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User)
                .WithMany(p => p.Follows)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FollowedId 不設定外鍵關聯，由商業邏輯處理
            // 註釋：FollowedId 可能指向不同類型的實體，例如 Person、Article 等，因此不設定外鍵關聯

            modelBuilder.ApplyConfiguration(new NFTConfiguration());
        }

        /// <summary>
        /// 使用者資料表
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// 個人資料表
        /// </summary>
        public DbSet<Person> Persons { get; set; } = null!;

        /// <summary>
        /// 文章資料表
        /// </summary>
        public DbSet<Article> Articles { get; set; } = null!;

        /// <summary>
        /// 回覆資料表
        /// </summary>
        public DbSet<Reply> Replies { get; set; } = null!;

        /// <summary>
        /// 讚美收藏資料表
        /// </summary>
        public DbSet<PraiseCollect> PraiseCollects { get; set; } = null!;

        /// <summary>
        /// 追蹤資料表
        /// </summary>
        public DbSet<Follow> Follows { get; set; } = null!;

        /// <summary>
        /// 通知資料表
        /// </summary>
        public DbSet<Notification> Notifications { get; set; } = null!;

        /// <summary>
        /// 檢舉資料表
        /// </summary>
        public DbSet<Report> Reports { get; set; } = null!;

        /// <summary>
        /// 登入記錄資料表
        /// </summary>
        public DbSet<LoginRecord> LoginRecords { get; set; } = null!;

        /// <summary>
        /// 標籤資料表
        /// </summary>
        public DbSet<Hashtag> Hashtags { get; set; } = null!;

        /// <summary>
        /// 文章標籤關聯資料表
        /// </summary>
        public DbSet<ArticleHashtag> ArticleHashtags { get; set; } = null!;

        /// <summary>
        /// 好友關係資料表
        /// </summary>
        public DbSet<Friendship> Friendships { get; set; } = null!;

        /// <summary>
        /// 文章附件資料表
        /// </summary>
        public DbSet<ArticleAttachment> ArticleAttachments { get; set; } = null!;

        /// <summary>
        /// NFT 收藏資料表
        /// </summary>
        public DbSet<NFT> NFTs { get; set; } = null!;
    }
}