
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.IRepositories;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Infrastructure.Data.DBContext;
using ContentManagementService.Infrastructure.Data.Repositories;

namespace ContentManagementService.Application.UnitOfWork
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
