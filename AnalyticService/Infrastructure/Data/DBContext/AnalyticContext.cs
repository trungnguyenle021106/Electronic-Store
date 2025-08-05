using AnalyticService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnalyticService.Infrastructure.Data.DBContext
{
    public class AnalyticContext : DbContext
    {
        public AnalyticContext(DbContextOptions<AnalyticContext> options) : base(options)
        {
        }

        // DbSet cho bảng thống kê đơn hàng
        public DbSet<OrderByDate> OrdersByDates { get; set; }

        // DbSet cho bảng thống kê sản phẩm
        public DbSet<ProductStatistics> ProductsStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình cho bảng OrdersByDate
            modelBuilder.Entity<OrderByDate>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.Date).IsUnique();
                entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<ProductStatistics>(entity =>
            {
                entity.HasKey(e => e.ProductID);
                entity.Property(e => e.ProductID)
                .ValueGeneratedNever();

                //entity.HasIndex(e => e.ProductID).IsUnique();
            });
        }
    }
}
