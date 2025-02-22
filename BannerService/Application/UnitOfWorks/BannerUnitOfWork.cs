
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.IRepositories;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using BannerService.Infrastructure.Repository;

namespace BannerService.Application.UnitOfWork
{
    public class BannerUnitOfWork : IUnitOfWork
    {
        private readonly BannerContext _Context;
        private readonly IRepository<Banner> _BannerRepository;
        public BannerUnitOfWork(BannerContext context)
        {
            this._BannerRepository = new Repository<Banner>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }


        public IRepository<Banner> BannerRepository()
        {
            return this._BannerRepository;
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }
    }
}
