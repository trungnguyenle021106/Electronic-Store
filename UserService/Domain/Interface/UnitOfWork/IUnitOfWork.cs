using Microsoft.EntityFrameworkCore.Storage;
using UserService.Domain.Entities;
using UserService.Domain.Interface.IRepositories;

namespace UserService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Account> AccountRepository();
        IRepository<Customer> CustomerRepository();
        IRepository<RefreshToken> RefreshTokenRepository();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        Task Commit();
    }
}
