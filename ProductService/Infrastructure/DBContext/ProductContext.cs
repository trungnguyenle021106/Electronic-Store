
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.DBContext
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<ProductPropertyDetail> ProductPropertyDetails { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ProductPropertyDetail>(entity =>
            //{
            //    entity.HasKey(od => new { od.ProductID, od.ProductPropertyID });

            //    entity.HasOne(od => od.Product)
            //        .WithMany(o => o.ProductPropertyDetails)
            //        .HasForeignKey(od => od.ProductID);

            //    entity.HasOne(od => od.ProductProperty)
            //        .WithMany(o => o.ProductPropertyDetails)
            //        .HasForeignKey(od => od.ProductPropertyID);
            //});

            modelBuilder.Entity<Product>()
                .HasMany(e => e.ProductProperties)
                .WithMany(e => e.Products)
                .UsingEntity<ProductPropertyDetail>();
        }
    }
}
