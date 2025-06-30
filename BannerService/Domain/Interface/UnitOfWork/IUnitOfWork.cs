using BannerService.Domain.Entities;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.IRepositories;
using ContentManagementService.Domain.Entities;

namespace BannerService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Filter> FilterRepository();
        IRepository<FilterDetail> FilterDetailRepository();
        Task Commit();
        void Rollback();
    }
}
