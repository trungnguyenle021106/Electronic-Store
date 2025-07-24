using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OrderService.Domain.Entities;
using System.Reflection.Metadata;

namespace OrderService.Infrastructure.Data.DBContext
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
                modelBuilder.Entity<Order>()
                .HasMany<OrderDetail>()
                .WithOne();

            modelBuilder.Entity<OrderDetail>()
                .HasKey("OrderID", "ProductID");
        }
    }
}
