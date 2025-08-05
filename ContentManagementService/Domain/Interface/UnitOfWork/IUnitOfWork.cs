using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ContentManagementService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Filter> FilterRepository();
        IRepository<FilterDetail> FilterDetailRepository();
        Task RollbackAsync(IDbContextTransaction transaction);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task Commit();
        void Rollback();
    }
}
