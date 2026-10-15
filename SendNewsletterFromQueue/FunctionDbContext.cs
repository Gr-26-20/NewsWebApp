using Microsoft.EntityFrameworkCore;

namespace SendNewsletterFromQueue
{

    public sealed class FuncDbContext(DbContextOptions<FuncDbContext> options)
        : DbContext(options)
    {
        public DbSet<AspNetUser> AspNetUsers => Set<AspNetUser>();

        public DbSet<NewsLetter> NewsLetters { get; set; }

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

                entity.Property(user => user.IsSubscribedToNewsletter)
                    .IsRequired();

                
            });

            modelBuilder.Entity<NewsLetter>(entity =>
            {
                entity.ToTable("NewsLetters");
                entity.HasKey(newsletter => newsletter.Id);
                entity.Property(newsletter => newsletter.Subject)
                    .IsRequired()
                    .HasMaxLength(200);
                //entity.Property(newsletter => newsletter.Category)
                //    .IsRequired()
                //    .HasMaxLength(256);
                entity.Property(newsletter => newsletter.Body)
                    .IsRequired();
                entity.Property(newsletter => newsletter.Logo)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.imageUrl)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.Title)
                    .HasMaxLength(200);
                entity.Property(newsletter => newsletter.Description)
                    .HasMaxLength(1000);
                entity.Property(newsletter => newsletter.link)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.imageUrl2)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.Title2)
                    .HasMaxLength(200);
                entity.Property(newsletter => newsletter.Description2)
                    .HasMaxLength(1000);
                entity.Property(newsletter => newsletter.link2)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.imageUrl3)
                    .HasMaxLength(500);
                entity.Property(newsletter => newsletter.Title3)
                    .HasMaxLength(200);
                entity.Property(newsletter => newsletter.Description3)
                    .HasMaxLength(1000);
                entity.Property(newsletter => newsletter.link3)
                    .HasMaxLength(500);
            });
        }
    }



    public sealed class AspNetUser
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsSubscribedToNewsletter { get; set; } 

        //public int YearOfBirth { get; set; }
    }


    public sealed class NewsLetter
    {
        public int Id { get; set; }

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? Email { get; set; }
        //public string? Category { get; set; }
        public string? imageUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? link { get; set; }
        public string? imageUrl2 { get; set; }
        public string? Title2 { get; set; }
        public string? Description2 { get; set; }
        public string? link2 { get; set; }
        public string? imageUrl3 { get; set; }
        public string? Title3 { get; set; }
        public string? Description3 { get; set; }
        public string? link3 { get; set; }
    }
}
