using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionReminder
{
    public sealed class FunctionDbContext(DbContextOptions<FunctionDbContext> options)
        : DbContext(options)
    {
        public DbSet<AspNetUser> AspNetUsers => Set<AspNetUser>();

        public DbSet<Subscriptions> Subscriptions => Set<Subscriptions>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Id)
                    .HasMaxLength(450);
                entity.Property(user => user.Email)
                    .HasMaxLength(256);
                
            });

            modelBuilder.Entity<Subscriptions>(entity =>
            {
                entity.ToTable("Subscriptions");
                entity.HasKey(subscription => subscription.Id);
                entity.Property(subscription => subscription.Id)
                    .HasMaxLength(450);
                entity.Property(subscription => subscription.UserId)
                    .HasMaxLength(450);
                entity.Property(subscription => subscription.StripeStatus)
                    .HasMaxLength(50);
                entity.Property(subscription => subscription.StripeInvoicePaid)
                    .IsRequired();
                entity.Property(subscription => subscription.CurrentPeriodEndUtc)
                    .IsRequired(false);
                entity.Property(subscription => subscription.User)
                    .IsRequired(false);
                entity.Property(subscription => subscription.SubscriptionEndDate);
                entity.HasOne(subscription => subscription.User)
                    .WithMany()
                    .HasForeignKey(subscription => subscription.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    
    }

    public sealed class AspNetUser
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public sealed class Subscriptions
    {
        public string Id { get; set; } 
        public string UserId { get; set; } 
        public string StripeStatus { get; set; } 
        public bool StripeInvoicePaid { get; set; }
        public DateTime? CurrentPeriodEndUtc { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime? SubscriptionEndDate { get; set; }
    }

    public sealed class ApplicationUser
    {
        public string? StripeCustomerId { get; set; }
        public string? StripeCheckoutSessionId { get; set; }
    }
}
