using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
    }
}
