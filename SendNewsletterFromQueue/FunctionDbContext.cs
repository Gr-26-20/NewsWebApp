using Microsoft.EntityFrameworkCore;

namespace SendNewsletterFromQueue;

public sealed class FuncDbContext(DbContextOptions<FuncDbContext> options) : DbContext(options)
{
    public DbSet<NewsletterUser> Users => Set<NewsletterUser>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsletterUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<NewsletterUser>().HasKey(u => u.Id);
        modelBuilder.Entity<NewsletterUser>().Property(u => u.Id).HasMaxLength(450);
        modelBuilder.Entity<NewsletterUser>().Property(u => u.Email).HasMaxLength(256);
    }
}

public sealed class NewsletterUser
{
    public string Id { get; set; } = "";
    public string? Email { get; set; }
    public bool NewsletterOptIn { get; set; }
    public DateTime? LastNewsletterAttemptUtc { get; set; }
}
