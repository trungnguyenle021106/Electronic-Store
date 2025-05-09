using BannerService.Domain.Entities;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.IRepositories;

namespace BannerService.Domain.Interface.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Banner> BannerRepository();
        Task Commit();
        void Rollback();
    }
}
