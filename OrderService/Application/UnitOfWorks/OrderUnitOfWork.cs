
using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.IRepositories;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.Data.DBContext;
using OrderService.Infrastructure.Data.Repositories;

namespace OrderService.Application.UnitOfWork
{
    public class OrderUnitOfWork : IUnitOfWork
    {
        private readonly OrderContext _Context;
        private readonly IRepository<Order> _OrderRepository;
        private readonly IRepository<OrderDetail> _OrderDetailRepository;
        public OrderUnitOfWork(OrderContext context)
        {
            this._OrderRepository = new Repository<Order>(context);
            this._OrderDetailRepository = new Repository<OrderDetail>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }

        public IRepository<OrderDetail> OrderDetailRepository()
        {
            return this._OrderDetailRepository;
        }

        public IRepository<Order> OrderRepository()
        {
            return this._OrderRepository;
        }

        public async Task RollbackAsync(IDbContextTransaction transaction)
        {

            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
        }
        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _Context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction) // <-- Triển khai
        {
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }
        }

    }
}
