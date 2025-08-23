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

            // 套用所有實體的配置
            modelBuilder.ApplyConfiguration(new ArticleHashtagConfiguration());
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new ReportConfiguration());
            modelBuilder.ApplyConfiguration(new PraiseCollectConfiguration());
            modelBuilder.ApplyConfiguration(new ReplyConfiguration());
            modelBuilder.ApplyConfiguration(new FriendshipConfiguration());
            modelBuilder.ApplyConfiguration(new ArticleAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new FollowConfiguration());
            modelBuilder.ApplyConfiguration(new NFTConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
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

        /// <summary>
        /// 聊天資料表
        /// </summary>
        public DbSet<Message> Messages { get; set; } = null!;
    }
}