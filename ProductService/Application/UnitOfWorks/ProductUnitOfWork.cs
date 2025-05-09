
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.IRepositories;
using ProductService.Domain.Interface.UnitOfWork;
using ProductService.Infrastructure.DBContext;
using ProductService.Infrastructure.Repository;

namespace ProductService.Application.UnitOfWork
{
    public class ProductUnitOfWork : IUnitOfWork
    {
        private readonly ProductContext _Context;
        private readonly IRepository<Product> _ProductRepository;
        private readonly IRepository<ProductProperty> _ProductPropertyRepository;
        private readonly IRepository<ProductPropertyDetail> _ProductPropertyDetailRepository;
        public ProductUnitOfWork(ProductContext context)
        {
            this._ProductRepository = new Repository<Product>(context);
            this._ProductPropertyRepository = new Repository<ProductProperty>(context);
            this._ProductPropertyDetailRepository = new Repository<ProductPropertyDetail>(context);
            this._Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public IRepository<ProductPropertyDetail> ProductPropertyDetailRepository()
        {
           return this._ProductPropertyDetailRepository;
        }

        public IRepository<ProductProperty> ProductPropertyRepository()
        {
            return this._ProductPropertyRepository;
        }

        public IRepository<Product> ProductRepository()
        {
            return this._ProductRepository;
        }
    }
}
