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

    }
}
