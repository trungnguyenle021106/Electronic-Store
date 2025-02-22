using Microsoft.EntityFrameworkCore;
using BannerService.Domain.Entities;

namespace BannerService.Infrastructure.DBContext
{
    public class BannerContext : DbContext
    {
        public BannerContext(DbContextOptions<BannerContext> options) : base(options)
        {
        }

        public DbSet<Banner> Banners { get; set; }

    }
}
