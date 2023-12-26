using Mazda.Model;
using Microsoft.EntityFrameworkCore;

namespace Mazda
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options) { }
        public DbSet<AboutUS> AboutUs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryType> CategoryTypes { get; set; }
        public DbSet<ForgotPass> ForgotPasses { get; set; }
        public DbSet<Guide> Guides { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Button> Buttons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryType>().HasData(
                new CategoryType { Id = 1, Name = "Sản phẩm" },
                new CategoryType { Id = 2, Name = "Tin tức" },
                new CategoryType { Id = 3, Name = " Hướng dẫn" }
            );
        }

    }
}
