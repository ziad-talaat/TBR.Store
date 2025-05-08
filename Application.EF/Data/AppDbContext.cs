using Microsoft.EntityFrameworkCore;
using TBL.Core.Models;

namespace Application.EF.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions options):base(options)
        {
            
        }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
