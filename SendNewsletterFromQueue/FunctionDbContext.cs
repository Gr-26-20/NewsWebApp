using Microsoft.EntityFrameworkCore;

namespace SendNewsletterFromQueue
{

    public sealed class FuncDbContext(DbContextOptions<FuncDbContext> options)
        : DbContext(options)
    {
        public DbSet<AspNetUser> AspNetUsers => Set<AspNetUser>();

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

                entity.Property(user => user.YearOfBirth)
                    .IsRequired();
            });
        }
    }



    public sealed class AspNetUser
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public int YearOfBirth { get; set; }
    }

}
