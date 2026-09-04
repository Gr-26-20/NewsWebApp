using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Models;

namespace NewsWebApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Articles> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subscriptions> Subscriptions { get; set; }
        public DbSet<Weather> Weather { get; set; }
    }
}
