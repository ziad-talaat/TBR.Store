using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TBL.Core.Models;

namespace Application.EF.Data
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions options):base(options)
        {
            
        }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Company> Company { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);





            modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.UserName).IsUnique();
            modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.Email).IsUnique();

            modelBuilder.Entity<Product>().HasMany(x => x.Users).WithMany(x => x.Products).UsingEntity<UserProduct_Voting>(
                l => l
                .HasOne(x => x.User)
                .WithMany(x => x.UserProduct_Voting)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade),
                r => r.
                HasOne(x => x.Product)
                .WithMany(x => x.UserProduct_Voting)
                .HasForeignKey(x => x.ProductId)
                 .OnDelete(DeleteBehavior.Cascade),

               join =>
               {
                   join.HasKey(uv => new { uv.ProductId, uv.UserId });
               }
                );

            modelBuilder.Entity<ApplicationUser>()
             .Property(u => u.Id)
             .HasDefaultValueSql("NEWID()");
        }
    }
}
