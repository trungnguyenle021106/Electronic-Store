using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace ContentManagementService.Infrastructure.Data.DBContext
{
    public class ContentManagementContext : DbContext
    {
        public ContentManagementContext(DbContextOptions<ContentManagementContext> options) : base(options)
        {
        }

        public DbSet<Filter> Filters { get; set; }
        public DbSet<FilterDetail> FilterDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterDetail>()
           .HasKey(fd => new { fd.FilterID, fd.ProductPropertyID });

            modelBuilder.Entity<Filter>()
           .HasMany(f => f.FilterDetails)
           .WithOne(fd => fd.Filter)
           .HasForeignKey(fd => fd.FilterID);      

        }
    }
}
