using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.IRepositories;
using ContentManagementService.Domain.Entities;

namespace ContentManagementService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Filter> FilterRepository();
        IRepository<FilterDetail> FilterDetailRepository();
        Task Commit();
        void Rollback();
    }
}
