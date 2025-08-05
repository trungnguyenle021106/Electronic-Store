
using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Interface.IRepositories;
using AnalyticService.Domain.Interface.UnitOfWork;
using AnalyticService.Infrastructure.Data.DBContext;
using AnalyticService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnalyticService.Application.UnitOfWork
{
    public class AnalyticUnitOfWork : IUnitOfWork
    {
        private readonly AnalyticContext _Context;
        private readonly IRepository<OrderByDate> _OrderByDateRepository;
        private readonly IRepository<ProductStatistics> _ProductStatisticsRepository;

        public AnalyticUnitOfWork(AnalyticContext context)
        {
            this._OrderByDateRepository = new Repository<OrderByDate>(context);
            this._ProductStatisticsRepository = new Repository<ProductStatistics>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }


        public IRepository<OrderByDate> OrderByDateRepository()
        {
            return this._OrderByDateRepository;
        }

        public IRepository<ProductStatistics> ProductStatisticsRepository()
        {
            return this._ProductStatisticsRepository;
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public async Task RollbackAsync(IDbContextTransaction transaction)
        {

            if (transaction != null)
            {
                await transaction.RollbackAsync();
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
