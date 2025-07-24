using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.IRepositories;

namespace OrderService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Order> OrderRepository();
        IRepository<OrderDetail> OrderDetailRepository();
        Task RollbackAsync(IDbContextTransaction transaction);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task Commit();
        void Rollback();
    }
}
