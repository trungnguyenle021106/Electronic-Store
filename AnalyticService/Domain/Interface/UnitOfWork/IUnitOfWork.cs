using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Interface.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnalyticService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<OrderByDate> OrderByDateRepository();
        IRepository<ProductStatistics> ProductStatisticsRepository();
        Task RollbackAsync(IDbContextTransaction transaction);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task Commit();
        void Rollback();
    }
}
