// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Matrix.Models;
// using Microsoft.AspNetCore.Identity;

namespace Matrix.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ArticleHashtag 複合主鍵
        modelBuilder.Entity<ArticleHashtag>()
            .HasKey(ah => new { ah.ArticleId, ah.TagId });

        // Person 一對一 User
        modelBuilder.Entity<Person>()
            .HasOne(p => p.User)
            .WithOne(u => u.Person)
            .HasForeignKey<Person>(p => p.UserId)
            .IsRequired();  // 確保關聯是必須的

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Receiver)
            .WithMany(p => p.NotificationsReceived)
            .HasForeignKey(n => n.GetId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Sender)
            .WithMany(p => p.NotificationsSent)
            .HasForeignKey(n => n.SendId)
            .OnDelete(DeleteBehavior.Restrict);

        // Person - Report (ReportsMade)
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany(p => p.ReportsMade)
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Person - Report (ReportsResolved)
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Resolver)
            .WithMany(p => p.ReportsResolved)
            .HasForeignKey(r => r.ResolverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PraiseCollect>()
            .HasOne(pc => pc.User)
            .WithMany(p => p.PraiseCollects)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

        modelBuilder.Entity<PraiseCollect>()
            .HasOne(pc => pc.Article)
            .WithMany(a => a.PraiseCollects)
            .HasForeignKey(pc => pc.ArticleId)
            .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

        modelBuilder.Entity<Reply>()
            .HasOne(r => r.User)
            .WithMany(p => p.Replies)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict); // 或 NoAction

        modelBuilder.Entity<Reply>()
            .HasOne(r => r.Article)
            .WithMany(a => a.Replies)
            .HasForeignKey(r => r.ArticleId)
            .OnDelete(DeleteBehavior.Cascade); // 這個可以保留

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(p => p.Friends)      // 這裡指定 Person.Friends
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Recipient)
            .WithMany(p => p.FriendOf)     // 這裡指定 Person.FriendOf
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Article - ArticleAttachment
        modelBuilder.Entity<ArticleAttachment>()
            .HasOne(aa => aa.Article)
            .WithMany(a => a.Attachments)
            .HasForeignKey(aa => aa.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Person - Follow
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.User)
            .WithMany(p => p.Follows)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // FollowedId 不設定外鍵關聯，由商業邏輯處理
        // 註釋：FollowedId 可能指向不同類型的實體，例如 Person、Article 等，因此不設定外鍵關聯
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Reply> Replies { get; set; }
    public DbSet<PraiseCollect> PraiseCollects { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<LoginRecord> LoginRecords { get; set; }
    public DbSet<Hashtag> Hashtags { get; set; }
    public DbSet<ArticleHashtag> ArticleHashtags { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<ArticleAttachment> ArticleAttachments { get; set; }
}