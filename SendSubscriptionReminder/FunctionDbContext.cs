using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SendSubscriptionReminder
{
    public sealed class FunctionDbContext(DbContextOptions<FunctionDbContext> options) 
        : DbContext(options)
    {
        public DbSet<AspNetUser> AspNetUsers => Set<AspNetUser>();

        public DbSet<Subscriptions> Subscriptions => Set<Subscriptions>();

        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
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
                entity.Property(subscription => subscription.DurationInDays)
                    .IsRequired();
                entity.HasOne(subscription => subscription.User)
                    .WithMany()
                    .HasForeignKey(subscription => subscription.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    public sealed class AspNetUser
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public sealed class Subscriptions
    {
        public string Id { get; set; }

        public int DurationInDays { get; set; }

        public ApplicationUser User { get; set; }

        public string? UserId { get; set; }

        public DateTime? CurrentPeriodEndUtc { get; set; }

    }

    public sealed class ApplicationUser : IdentityUser
    {
        public string? StripeCustomerId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

    }
}


