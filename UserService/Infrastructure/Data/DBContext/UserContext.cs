
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.DBContext
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasOne(e => e.Customer)
                .WithOne(e => e.Account)
                .HasForeignKey<Customer>(e => e.AccountID)
                .IsRequired(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.RefreshTokens)
                .WithOne(e => e.Account)
                .HasForeignKey(e => e.AccountID)
                .IsRequired(false);
        }
    }

}
