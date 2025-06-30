using Microsoft.EntityFrameworkCore;
using BannerService.Domain.Entities;

namespace BannerService.Infrastructure.DBContext
{
    public class ContentManagementContext : DbContext
    {
        public ContentManagementContext(DbContextOptions<ContentManagementContext> options) : base(options)
        {
        }

        public DbSet<Filter> Banners { get; set; }

    }
}
