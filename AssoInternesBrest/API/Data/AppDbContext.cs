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
        public DbSet<BureauMember> BureauMembers { get; set; }
        public DbSet<GuidePage> GuidePages { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasData(new AppSetting
                {
                    Key = "contact_email",
                    Value = "contact@asso-internes-brest.fr"
                });
            });
        }
    }
}
