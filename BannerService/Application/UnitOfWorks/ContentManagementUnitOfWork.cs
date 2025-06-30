
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.IRepositories;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using BannerService.Infrastructure.Repository;
using ContentManagementService.Domain.Entities;

namespace BannerService.Application.UnitOfWork
{
    public class ContentManagementUnitOfWork : IUnitOfWork
    {
        private readonly ContentManagementContext _Context;
        private readonly IRepository<Filter> _FilterRepository;
        private readonly IRepository<FilterDetail> _FilterDetailRepository;

        public ContentManagementUnitOfWork(ContentManagementContext context)
        {
            this._FilterRepository = new Repository<Filter>(context);
            this._FilterDetailRepository = new Repository<FilterDetail>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }


        public IRepository<Filter> FilterRepository()
        {
            return this._FilterRepository;
        }

        public IRepository<FilterDetail> FilterDetailRepository()
        {
            return this._FilterDetailRepository;
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
