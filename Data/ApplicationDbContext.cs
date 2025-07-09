using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Matrix.Models;
using Microsoft.AspNetCore.Identity;

namespace Matrix.Data;

public class ApplicationDbContext : IdentityDbContext
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

        // Person 一對一 AspNetUser
        modelBuilder.Entity<Person>()
            .HasOne<IdentityUser>(p => p.User)
            .WithOne()
            .HasForeignKey<Person>(p => p.PersonId)
            .IsRequired();

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
    }

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
}