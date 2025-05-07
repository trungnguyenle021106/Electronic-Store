using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OrderService.Domain.Entities;
using System.Reflection.Metadata;

namespace OrderService.Infrastructure.DBContext
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<OrderDetail>(entity =>
            //{
            //    entity.HasKey(od => new { od.OrderID, od.ProductID });

            //    entity.HasOne(od => od.Order)
            //        .WithMany(o => o.OrderDetails)
            //        .HasForeignKey(od => od.OrderID); 
            //});

                modelBuilder.Entity<Order>()
                .HasMany<OrderDetail>()
                .WithOne();

            modelBuilder.Entity<OrderDetail>()
                .HasKey("OrderID", "ProductID");
        }
    }
}
